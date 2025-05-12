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
	- Member Container
  - VB container
    - Definition
    - Extras
    - Docs
    - Commons
	- Member Container

- Definition
  - One for each symbol
  - Language-agnostic: IPreprocessingSymbol
    - Include base type / interface list for type symbols
- Extras
  - Type parameter constraint list (only for C#)
  - Preprocessing symbol definition state
    - '[Un]Defined here' for both langs (we cannot retrieve the defined value in VB)
  - Function pointer signature showcase
- Docs
  - Common for both languages, assuming XML documentation is common for both languages
    Show supported tags like summary, remarks, etc.
- Commons
  - Simple reference to another symbol, almost equivalent to minimal display string
    This will be used by all the other creators to refer to another symbol without
    expanding on its extras
	This is most useful for types being described anywhere, and referred symbols in
	the docs
- Member Container
  - Displays information about the containers the member is a part of
    - Member symbols (modules, namespaces, types, methods, fields, properties and events)
	  display their containers' information fully
    - Non-members (locals, parameters, etc.) may contain limited information about their
	  containing symbol; preferably the method they are contained in

#endif
