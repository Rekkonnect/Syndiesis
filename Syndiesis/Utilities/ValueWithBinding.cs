using Avalonia.Data;
using System.Reactive.Subjects;

namespace Syndiesis.Utilities;

using BindingSubject = ISubject<BindingValue<object?>>;

public record ValueWithBinding(object? CurrentValue, BindingSubject BindingSubject);
