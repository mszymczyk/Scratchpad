It's a collection of few test programs that served as proof of concepts. Some of them were improved and used in production.

### ZMQHub - service that glues all tools together
 - passes messages among tools
 - simple plain text protocol
 - based on ZeroMQ

### FxCompiler - custom shader effect file compiler
 - replacement of Dx10 effect files
 - can support different languages, HLSL\GLSL\PSSL (GLSL and PSSL not added in this branch)
 - visual studio integration
 - configurations: Debug, Release, Diagnostic, Shipping
 - generates multiple shader permutations based on preprocessor defines
 - allows passing native compiler args from within effect file
 - multithreaded compilation
 - can output diagnostic\performance info

### SettingsEditor - tool for managing large number of game settings
 - schema written in C#
 - generates C++ header for inclusion in game code
 - supported setting types: int, float, enum, string, float3, color, direction, anim curve, string array
 - presets
 - edit history
 - quick search in all open documents
 - supports custom window layouts
 
### MaterialEditor - node base shader\material editor
 - instant preview in engine
 - material instances
 - templates

### AnimClipEditor - tool for tagging animations
