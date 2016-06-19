#ifndef HLSLFRAMEWORKINTEROP_H
#define HLSLFRAMEWORKINTEROP_H

#ifdef __cplusplus

#include <Util\Vectormath.h>

typedef Matrix4 float4x4;
typedef Vector4 float4;

#define CBUFFER struct
#define REGISTER_B(exp)

#else

#define REGISTER_B(exp) : register(b##exp) // constant buffer

#define CBUFFER cbuffer

#endif //

#endif // HLSLFRAMEWORKINTEROP_H
