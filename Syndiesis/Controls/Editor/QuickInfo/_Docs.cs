#undef __SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED

#if __SECRET_INTERNALS_DO_NOT_USE_OR_YOU_WILL_BE_FIRED

Hierarchy

- Hybrid language container
- Language-specific container
- UI part-specific container
- Symbol-specific creator

If creators can be language-agnostic per symbol type, they are therefore
included in the base container definition, wasting extra instances for the
same type, but it's fine because they act as singleton instances, not storing
any state and being very lightweight in memory.

The planned structure is:
- Hybrid container
  - C# container
    - Definition
    - Extras
    - Docs
    - Commons
  - VB container
    - Definition
    - Extras
    - Docs
    - Commons

- Definition
  - One for each symbol
  - Language-agnostic: IPreprocessingSymbol
    - Include base type / interface list for type symbols
- Extras
  - Type parameter constraint list (only for C#)
  - Preprocessing symbol definition state
    - '[Un]Defined here' for C#
    - '[Un]Defined here [as XYZ]' for VB
- Docs
  - Common for both languages, assuming XML documentation is common for both languages
    Show supported tags like summary, remarks, etc.
- Commons
  - Simple reference to another symbol, almost equivalent to minimal display string
    This will be used by all the other creators to refer to another symbol without
    expanding on its extras

#endif
