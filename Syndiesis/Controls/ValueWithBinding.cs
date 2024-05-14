using Avalonia.Data;
using System.Reactive.Subjects;

namespace Syndiesis.Controls;

using BindingSubject = ISubject<BindingValue<object?>>;

public record ValueWithBinding(object? CurrentValue, BindingSubject BindingSubject);
