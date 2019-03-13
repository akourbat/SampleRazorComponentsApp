using FluentValidation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace DatingApp.Server.ViewModels
{
    public class ReactiveFodyViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> Clear { get; }

        [ObservableAsProperty]
        public string Greeting { get; }

        [ObservableAsProperty]
        public IEnumerable<string> Results { get; }

        [ObservableAsProperty]
        public bool HasErrors { get; }

        [ObservableAsProperty]
        public bool CanExecute { get; }

        [Reactive]
        public string Name { get; set; } = string.Empty;

        private NameValidator Validator { get; }

        public ReactiveFodyViewModel()
        {
            this.WhenAnyValue(x => x.Name, (name) => !string.IsNullOrEmpty(name) && name.Length >= 4 )
                .ToPropertyEx(this, x => x.CanExecute);

            Clear = ReactiveCommand.Create(() => { this.Name = string.Empty; }, this.WhenAnyValue(x => x.CanExecute));

            this.WhenAnyValue(x => x.Name)
                .Select(name => $"Hello, {name}!")
                .ToPropertyEx(this, x => x.Greeting);

            this.Validator = new NameValidator();

            this.WhenAnyValue(x => x.Name)
                .Throttle(TimeSpan.FromMilliseconds(800))
                .DistinctUntilChanged()
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => Validator.Validate(this).Errors.Select(e => e.ErrorMessage))
                .ToPropertyEx(this, x => x.Results);

            this.WhenAnyValue(x => x.Results, l => l != null && l.Any())
                .ToPropertyEx(this, x => x.HasErrors);
        }
    }

    public class NameValidator : AbstractValidator<ReactiveFodyViewModel>
    {
        public NameValidator()
        {
            RuleFor(n => n.Name).MaximumLength(8).WithMessage("Hey it is too long, dude!");
        }
    }
}
