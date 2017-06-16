It's a collection of few test programs that served as proof of concepts. Some of them were improved and used in production.

### FxCompiler - custom shader effect file compiler
 - replacement of Dx10 effect files
 - can support different languages, HLSL\GLSL\PSSL (GLSL and PSSL not added in this branch)
 - visual studio integration
 - configurations: Debug, Release, Diagnostic, Shipping
 - generates multiple shader permutations based on preprocessor defines
 - allows passing native compiler args from within effect file
 - multithreaded compilation
 - can output diagnostic\performance info
