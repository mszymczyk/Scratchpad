/*
   Copyright (C) 2006, 2007 Sony Computer Entertainment Inc.
   All rights reserved.

   Redistribution and use in source and binary forms,
   with or without modification, are permitted provided that the
   following conditions are met:
    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
      notice, this list of conditions and the following disclaimer in the
      documentation and/or other materials provided with the distribution.
    * Neither the name of the Sony Computer Entertainment Inc nor the names
      of its contributors may be used to endorse or promote products derived
      from this software without specific prior written permission.

   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
   AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
   IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
   ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
   LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
   CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
   SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
   INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
   CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
   ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
   POSSIBILITY OF SUCH DAMAGE.
*/

#ifndef _VECTORMATH_VEC_SOA_CPP_H
#define _VECTORMATH_VEC_SOA_CPP_H
//-----------------------------------------------------------------------------
// Constants
// for shuffles, words are labeled [x,y,z,w] [a,b,c,d]

#define _VECTORMATH_SHUF_X 0x00010203
#define _VECTORMATH_SHUF_Y 0x04050607
#define _VECTORMATH_SHUF_Z 0x08090a0b
#define _VECTORMATH_SHUF_W 0x0c0d0e0f
#define _VECTORMATH_SHUF_A 0x10111213
#define _VECTORMATH_SHUF_B 0x14151617
#define _VECTORMATH_SHUF_C 0x18191a1b
#define _VECTORMATH_SHUF_D 0x1c1d1e1f
#define _VECTORMATH_SHUF_0 0x80808080
#define _VECTORMATH_SHUF_XAYB ((vec_uchar16)(vec_uint4){ _VECTORMATH_SHUF_X, _VECTORMATH_SHUF_A, _VECTORMATH_SHUF_Y, _VECTORMATH_SHUF_B })
#define _VECTORMATH_SHUF_ZCWD ((vec_uchar16)(vec_uint4){ _VECTORMATH_SHUF_Z, _VECTORMATH_SHUF_C, _VECTORMATH_SHUF_W, _VECTORMATH_SHUF_D })
#define _VECTORMATH_SHUF_ZBW0 ((vec_uchar16)(vec_uint4){ _VECTORMATH_SHUF_Z, _VECTORMATH_SHUF_B, _VECTORMATH_SHUF_W, _VECTORMATH_SHUF_0 })
#define _VECTORMATH_SHUF_XCY0 ((vec_uchar16)(vec_uint4){ _VECTORMATH_SHUF_X, _VECTORMATH_SHUF_C, _VECTORMATH_SHUF_Y, _VECTORMATH_SHUF_0 })
#define _VECTORMATH_SHUF_ZDW0 ((vec_uchar16)(vec_uint4){ _VECTORMATH_SHUF_Z, _VECTORMATH_SHUF_D, _VECTORMATH_SHUF_W, _VECTORMATH_SHUF_0 })
#define _VECTORMATH_SHUF_XAZC ((vec_uchar16)(vec_uint4){ _VECTORMATH_SHUF_X, _VECTORMATH_SHUF_A, _VECTORMATH_SHUF_Z, _VECTORMATH_SHUF_C })
#define _VECTORMATH_SHUF_ZDXB ((vec_uchar16)(vec_uint4){ _VECTORMATH_SHUF_Z, _VECTORMATH_SHUF_D, _VECTORMATH_SHUF_X, _VECTORMATH_SHUF_B })
#define _VECTORMATH_SHUF_YBWD ((vec_uchar16)(vec_uint4){ _VECTORMATH_SHUF_Y, _VECTORMATH_SHUF_B, _VECTORMATH_SHUF_W, _VECTORMATH_SHUF_D })
#define _VECTORMATH_SHUF_XDZB ((vec_uchar16)(vec_uint4){ _VECTORMATH_SHUF_X, _VECTORMATH_SHUF_D, _VECTORMATH_SHUF_Z, _VECTORMATH_SHUF_B })
#define _VECTORMATH_SHUF_YAWC ((vec_uchar16)(vec_uint4){ _VECTORMATH_SHUF_Y, _VECTORMATH_SHUF_A, _VECTORMATH_SHUF_W, _VECTORMATH_SHUF_C })
#define _VECTORMATH_SHUF_ZBXD ((vec_uchar16)(vec_uint4){ _VECTORMATH_SHUF_Z, _VECTORMATH_SHUF_B, _VECTORMATH_SHUF_X, _VECTORMATH_SHUF_D })
#define _VECTORMATH_SHUF_XYCD ((vec_uchar16)(vec_uint4){ _VECTORMATH_SHUF_X, _VECTORMATH_SHUF_Y, _VECTORMATH_SHUF_C, _VECTORMATH_SHUF_D })
#define _VECTORMATH_SLERP_TOL 0.999f
//-----------------------------------------------------------------------------
// Definitions

#ifndef _VECTORMATH_INTERNAL_FUNCTIONS
#define _VECTORMATH_INTERNAL_FUNCTIONS

#endif

namespace Vectormath {
namespace Soa {

inline Vector3::Vector3( const Vector3 & vec )
{
    mX = vec.mX;
    mY = vec.mY;
    mZ = vec.mZ;
}

inline Vector3::Vector3( vec_float4 _x, vec_float4 _y, vec_float4 _z )
{
    mX = _x;
    mY = _y;
    mZ = _z;
}

inline Vector3::Vector3( const Point3 & pnt )
{
    mX = pnt.getX();
    mY = pnt.getY();
    mZ = pnt.getZ();
}

inline Vector3::Vector3( vec_float4 scalar )
{
    mX = scalar;
    mY = scalar;
    mZ = scalar;
}

inline Vector3::Vector3( const Aos::Vector3 & vec )
{
    //vec_uchar16 shuffle_xxxx = (vec_uchar16)_MM_SHUFFLE((int)0x00010203);
    //vec_uchar16 shuffle_yyyy = (vec_uchar16)vec_splats((int)0x04050607);
    //vec_uchar16 shuffle_zzzz = (vec_uchar16)vec_splats((int)0x08090a0b);
    //vec_float4 vec128 = vec.get128();
    //mX = vec_shuffle( vec128, vec128, shuffle_xxxx );
    //mY = vec_shuffle( vec128, vec128, shuffle_yyyy );
    //mZ = vec_shuffle( vec128, vec128, shuffle_zzzz );

    mX = vec.get128();
    mY = vec.get128();
    mZ = vec.get128();
    vec_float4 tmp = vec.get128();
    _MM_TRANSPOSE4_PS( mX, mY, mZ, tmp );
}

inline Vector3::Vector3( const Aos::Vector3 & vec0, const Aos::Vector3 & vec1, const Aos::Vector3 & vec2, const Aos::Vector3 & vec3 )
{
    mX = vec0.get128();
    mY = vec1.get128();
    mZ = vec2.get128();
    vec_float4 tmp = vec3.get128();
    _MM_TRANSPOSE4_PS( mX, mY, mZ, tmp );


    //tmp0 = spu_shuffle( vec0.get128(), vec2.get128(), _VECTORMATH_SHUF_XAYB );
    //tmp1 = spu_shuffle( vec1.get128(), vec3.get128(), _VECTORMATH_SHUF_XAYB );
    //tmp2 = spu_shuffle( vec0.get128(), vec2.get128(), _VECTORMATH_SHUF_ZCWD );
    //tmp3 = spu_shuffle( vec1.get128(), vec3.get128(), _VECTORMATH_SHUF_ZCWD );
    //mX = spu_shuffle( tmp0, tmp1, _VECTORMATH_SHUF_XAYB );
    //mY = spu_shuffle( tmp0, tmp1, _VECTORMATH_SHUF_ZCWD );
    //mZ = spu_shuffle( tmp2, tmp3, _VECTORMATH_SHUF_XAYB );
}

inline const Vector3 Vector3::xAxis( )
{
    return Vector3( vec_splats(1.0f), vec_splats(0.0f), vec_splats(0.0f) );
}

inline const Vector3 Vector3::yAxis( )
{
    return Vector3( vec_splats(0.0f), vec_splats(1.0f), vec_splats(0.0f) );
}

inline const Vector3 Vector3::zAxis( )
{
    return Vector3( vec_splats(0.0f), vec_splats(0.0f), vec_splats(1.0f) );
}

inline const Vector3 lerp( vec_float4 t, const Vector3 & vec0, const Vector3 & vec1 )
{
    return ( vec0 + ( ( vec1 - vec0 ) * t ) );
}

inline const Vector3 slerp( vec_float4 t, const Vector3 & unitVec0, const Vector3 & unitVec1 )
{
    vec_float4 recipSinAngle, scale0, scale1, cosAngle, angle;
    vec_uint4 selectMask;
    cosAngle = dot( unitVec0, unitVec1 );
    selectMask = (vec_uint4)vec_cmpgt( vec_splats(_VECTORMATH_SLERP_TOL), cosAngle );
    angle = acosf4( cosAngle );
	recipSinAngle = recipf4_newtonrapson( sinf4( angle ) );
    scale0 = vec_sel( vec_sub( vec_splats(1.0f), t ), vec_mul( sinf4( vec_mul( vec_sub( vec_splats(1.0f), t ), angle ) ), recipSinAngle ), selectMask );
    scale1 = vec_sel( t, vec_mul( sinf4( vec_mul( t, angle ) ), recipSinAngle ), selectMask );
    return ( ( unitVec0 * scale0 ) + ( unitVec1 * scale1 ) );
}

inline void Vector3::get4Aos( Aos::Vector3 & result0, Aos::Vector3 & result1, Aos::Vector3 & result2, Aos::Vector3 & result3 ) const
{
    vec_float4 vec0 = mX;
    vec_float4 vec1 = mY;
    vec_float4 vec2 = mZ;
    vec_float4 vec3 = { 0.f, 0.f, 0.f, 0.f };
    _MM_TRANSPOSE4_PS( vec0, vec1, vec2, vec3 );

    result0 = Aos::Vector3( vec0 );
    result1 = Aos::Vector3( vec1 );
    result2 = Aos::Vector3( vec2 );
    result3 = Aos::Vector3( vec3 );

    //tmp0 = spu_shuffle( mX, mZ, _VECTORMATH_SHUF_XAYB );
    //tmp1 = spu_shuffle( mX, mZ, _VECTORMATH_SHUF_ZCWD );
    //result0 = Aos::Vector3( spu_shuffle( tmp0, mY, _VECTORMATH_SHUF_XAYB ) );
    //result1 = Aos::Vector3( spu_shuffle( tmp0, mY, _VECTORMATH_SHUF_ZBW0 ) );
    //result2 = Aos::Vector3( spu_shuffle( tmp1, mY, _VECTORMATH_SHUF_XCY0 ) );
    //result3 = Aos::Vector3( spu_shuffle( tmp1, mY, _VECTORMATH_SHUF_ZDW0 ) );
}

inline void loadXYZArray( Vector3 & /*vec*/, const vec_float4 * /*threeQuads*/ )
{

    //vec_float4 xyxy, yzyz, zxzx, xyzx, yzxy, zxyz;
    //xyzx = threeQuads[0];
    //yzxy = threeQuads[1];
    //zxyz = threeQuads[2];
    //xyxy = spu_shuffle( xyzx, yzxy, _VECTORMATH_SHUF_XYCD );
    //zxzx = spu_shuffle( zxyz, xyzx, _VECTORMATH_SHUF_XYCD );
    //yzyz = spu_shuffle( yzxy, zxyz, _VECTORMATH_SHUF_XYCD );
    //vec.setX( spu_shuffle( xyxy, zxzx, _VECTORMATH_SHUF_XDZB ) );
    //vec.setY( spu_shuffle( xyxy, yzyz, _VECTORMATH_SHUF_YAWC ) );
    //vec.setZ( spu_shuffle( zxzx, yzyz, _VECTORMATH_SHUF_ZBXD ) );
}

inline void storeXYZArray( const Vector3 & /*vec*/, vec_float4 * /*threeQuads*/ )
{
    //vec_float4 xyzx, yzxy, zxyz, xyxy, zxzx, yzyz;
    //xyxy = spu_shuffle( vec.getX(), vec.getY(), _VECTORMATH_SHUF_XAZC );
    //zxzx = spu_shuffle( vec.getZ(), vec.getX(), _VECTORMATH_SHUF_ZDXB );
    //yzyz = spu_shuffle( vec.getY(), vec.getZ(), _VECTORMATH_SHUF_YBWD );
    //xyzx = spu_shuffle( xyxy, zxzx, _VECTORMATH_SHUF_XYCD );
    //yzxy = spu_shuffle( yzyz, xyxy, _VECTORMATH_SHUF_XYCD );
    //zxyz = spu_shuffle( zxzx, yzyz, _VECTORMATH_SHUF_XYCD );
    //threeQuads[0] = xyzx;
    //threeQuads[1] = yzxy;
    //threeQuads[2] = zxyz;
}

inline void storeHalfFloats( const Vector3 & /*vec0*/, const Vector3 & /*vec1*/, vec_ushort8 * /*threeQuads*/ )
{
    //vec_float4 xyz0[3];
    //vec_float4 xyz1[3];
    //storeXYZArray( vec0, xyz0 );
    //storeXYZArray( vec1, xyz1 );
    //threeQuads[0] = _vmath2VfToHalfFloats(xyz0[0], xyz0[1]);
    //threeQuads[1] = _vmath2VfToHalfFloats(xyz0[2], xyz1[0]);
    //threeQuads[2] = _vmath2VfToHalfFloats(xyz1[1], xyz1[2]);
}

inline Vector3 & Vector3::operator =( const Vector3 & vec )
{
    mX = vec.mX;
    mY = vec.mY;
    mZ = vec.mZ;
    return *this;
}

inline Vector3 & Vector3::setX( vec_float4 _x )
{
    mX = _x;
    return *this;
}

inline vec_float4 Vector3::getX( ) const
{
    return mX;
}

inline Vector3 & Vector3::setY( vec_float4 _y )
{
    mY = _y;
    return *this;
}

inline vec_float4 Vector3::getY( ) const
{
    return mY;
}

inline Vector3 & Vector3::setZ( vec_float4 _z )
{
    mZ = _z;
    return *this;
}

inline vec_float4 Vector3::getZ( ) const
{
    return mZ;
}

inline Vector3 & Vector3::setElem( int idx, vec_float4 value )
{
    *(&mX + idx) = value;
    return *this;
}

inline vec_float4 Vector3::getElem( int idx ) const
{
    return *(&mX + idx);
}

inline Vector3::vec_float4_t & Vector3::operator []( int idx )
{
    return *(&mX + idx);
}

inline vec_float4 Vector3::operator []( int idx ) const
{
    return *(&mX + idx);
}

inline const Vector3 Vector3::operator +( const Vector3 & vec ) const
{
    return Vector3(
        vec_add( mX, vec.mX ),
        vec_add( mY, vec.mY ),
        vec_add( mZ, vec.mZ )
    );
}

inline const Vector3 Vector3::operator -( const Vector3 & vec ) const
{
    return Vector3(
        vec_sub( mX, vec.mX ),
        vec_sub( mY, vec.mY ),
        vec_sub( mZ, vec.mZ )
    );
}

inline const Point3 Vector3::operator +( const Point3 & pnt ) const
{
    return Point3(
        vec_add( mX, pnt.getX() ),
        vec_add( mY, pnt.getY() ),
        vec_add( mZ, pnt.getZ() )
    );
}

inline const Vector3 Vector3::operator *( vec_float4 scalar ) const
{
    return Vector3(
        vec_mul( mX, scalar ),
        vec_mul( mY, scalar ),
        vec_mul( mZ, scalar )
    );
}

inline Vector3 & Vector3::operator +=( const Vector3 & vec )
{
    *this = *this + vec;
    return *this;
}

inline Vector3 & Vector3::operator -=( const Vector3 & vec )
{
    *this = *this - vec;
    return *this;
}

inline Vector3 & Vector3::operator *=( vec_float4 scalar )
{
    *this = *this * scalar;
    return *this;
}

inline const Vector3 Vector3::operator /( vec_float4 scalar ) const
{
    return Vector3(
        vec_div( mX, scalar ),
        vec_div( mY, scalar ),
        vec_div( mZ, scalar )
    );
}

inline Vector3 & Vector3::operator /=( vec_float4 scalar )
{
    *this = *this / scalar;
    return *this;
}

inline const Vector3 Vector3::operator -( ) const
{
    return Vector3(
        negatef4( mX ),
        negatef4( mY ),
        negatef4( mZ )
    );
}

inline const Vector3 operator *( vec_float4 scalar, const Vector3 & vec )
{
    return vec * scalar;
}

inline const Vector3 mulPerElem( const Vector3 & vec0, const Vector3 & vec1 )
{
    return Vector3(
        vec_mul( vec0.getX(), vec1.getX() ),
        vec_mul( vec0.getY(), vec1.getY() ),
        vec_mul( vec0.getZ(), vec1.getZ() )
    );
}

inline const Vector3 divPerElem( const Vector3 & vec0, const Vector3 & vec1 )
{
    return Vector3(
        vec_div( vec0.getX(), vec1.getX() ),
        vec_div( vec0.getY(), vec1.getY() ),
        vec_div( vec0.getZ(), vec1.getZ() )
    );
}

inline const Vector3 recipPerElem( const Vector3 & vec )
{
    return Vector3(
        recipf4( vec.getX() ),
        recipf4( vec.getY() ),
        recipf4( vec.getZ() )
    );
}

inline const Vector3 sqrtPerElem( const Vector3 & vec )
{
    return Vector3(
        sqrtf4( vec.getX() ),
        sqrtf4( vec.getY() ),
        sqrtf4( vec.getZ() )
    );
}

inline const Vector3 rsqrtPerElem( const Vector3 & vec )
{
    return Vector3(
        rsqrtf4( vec.getX() ),
        rsqrtf4( vec.getY() ),
        rsqrtf4( vec.getZ() )
    );
}

inline const Vector3 absPerElem( const Vector3 & vec )
{
    return Vector3(
        fabsf4( vec.getX() ),
        fabsf4( vec.getY() ),
        fabsf4( vec.getZ() )
    );
}

inline const Vector3 copySignPerElem( const Vector3 & vec0, const Vector3 & vec1 )
{
    return Vector3(
        copysignf4( vec0.getX(), vec1.getX() ),
        copysignf4( vec0.getY(), vec1.getY() ),
        copysignf4( vec0.getZ(), vec1.getZ() )
    );
}

inline const Vector3 maxPerElem( const Vector3 & vec0, const Vector3 & vec1 )
{
    return Vector3(
        vec_max( vec0.getX(), vec1.getX() ),
        vec_max( vec0.getY(), vec1.getY() ),
        vec_max( vec0.getZ(), vec1.getZ() )
    );
}

inline vec_float4 maxElem( const Vector3 & vec )
{
    vec_float4 result;
    result = vec_max( vec.getX(), vec.getY() );
    result = vec_max( vec.getZ(), result );
    return result;
}

inline const Vector3 minPerElem( const Vector3 & vec0, const Vector3 & vec1 )
{
    return Vector3(
        vec_min( vec0.getX(), vec1.getX() ),
        vec_min( vec0.getY(), vec1.getY() ),
        vec_min( vec0.getZ(), vec1.getZ() )
    );
}

inline vec_float4 minElem( const Vector3 & vec )
{
    vec_float4 result;
    result = vec_min( vec.getX(), vec.getY() );
    result = vec_min( vec.getZ(), result );
    return result;
}

inline vec_float4 sum( const Vector3 & vec )
{
    vec_float4 result;
    result = vec_add( vec.getX(), vec.getY() );
    result = vec_add( result, vec.getZ() );
    return result;
}

inline vec_float4 dot( const Vector3 & vec0, const Vector3 & vec1 )
{
    vec_float4 result;
    result = vec_mul( vec0.getX(), vec1.getX() );
    result = vec_add( result, vec_mul( vec0.getY(), vec1.getY() ) );
    result = vec_add( result, vec_mul( vec0.getZ(), vec1.getZ() ) );
    return result;
}

inline vec_float4 lengthSqr( const Vector3 & vec )
{
    vec_float4 result;
    result = vec_mul( vec.getX(), vec.getX() );
    result = vec_add( result, vec_mul( vec.getY(), vec.getY() ) );
    result = vec_add( result, vec_mul( vec.getZ(), vec.getZ() ) );
    return result;
}

inline vec_float4 length( const Vector3 & vec )
{
    return sqrtf4( lengthSqr( vec ) );
}

inline const Vector3 normalize( const Vector3 & vec )
{
    vec_float4 lenSqr, lenInv;
    lenSqr = lengthSqr( vec );
	lenInv = newtonrapson_rsqrt4( lenSqr );
    return Vector3(
        vec_mul( vec.getX(), lenInv ),
        vec_mul( vec.getY(), lenInv ),
        vec_mul( vec.getZ(), lenInv )
    );
}

inline const Vector3 cross( const Vector3 & vec0, const Vector3 & vec1 )
{
    return Vector3(
        vec_sub( vec_mul( vec0.getY(), vec1.getZ() ), vec_mul( vec0.getZ(), vec1.getY() ) ),
        vec_sub( vec_mul( vec0.getZ(), vec1.getX() ), vec_mul( vec0.getX(), vec1.getZ() ) ),
        vec_sub( vec_mul( vec0.getX(), vec1.getY() ), vec_mul( vec0.getY(), vec1.getX() ) )
    );
}

inline const Vector3 select( const Vector3 & vec0, const Vector3 & vec1, vec_uint4 select1 )
{
    return Vector3(
        vec_sel( vec0.getX(), vec1.getX(), select1 ),
        vec_sel( vec0.getY(), vec1.getY(), select1 ),
        vec_sel( vec0.getZ(), vec1.getZ(), select1 )
    );
}

#ifdef _VECTORMATH_DEBUG

inline void print( const Vector3 & vec )
{
    Aos::Vector3 vec0, vec1, vec2, vec3;
    vec.get4Aos( vec0, vec1, vec2, vec3 );
    printf("slot 0:\n");
    print( vec0 );
    printf("slot 1:\n");
    print( vec1 );
    printf("slot 2:\n");
    print( vec2 );
    printf("slot 3:\n");
    print( vec3 );
}

inline void print( const Vector3 & vec, const char * name )
{
    Aos::Vector3 vec0, vec1, vec2, vec3;
    printf( "%s:\n", name );
    vec.get4Aos( vec0, vec1, vec2, vec3 );
    printf("slot 0:\n");
    print( vec0 );
    printf("slot 1:\n");
    print( vec1 );
    printf("slot 2:\n");
    print( vec2 );
    printf("slot 3:\n");
    print( vec3 );
}

#endif

inline Vector4::Vector4( const Vector4 & vec )
{
    mX = vec.mX;
    mY = vec.mY;
    mZ = vec.mZ;
    mW = vec.mW;
}

inline Vector4::Vector4( const vec_float4 & _x, const vec_float4 & _y, const vec_float4 & _z, const vec_float4 & _w )
{
    mX = _x;
    mY = _y;
    mZ = _z;
    mW = _w;
}

inline Vector4::Vector4( const Vector3 & xyz, vec_float4 _w )
{
    this->setXYZ( xyz );
    this->setW( _w );
}

inline Vector4::Vector4( const Vector3 & vec )
{
    mX = vec.getX();
    mY = vec.getY();
    mZ = vec.getZ();
    mW = vec_splats(0.0f);
}

inline Vector4::Vector4( const Point3 & pnt )
{
    mX = pnt.getX();
    mY = pnt.getY();
    mZ = pnt.getZ();
    mW = vec_splats(1.0f);
}

inline Vector4::Vector4( const Quat & quat )
{
    mX = quat.getX();
    mY = quat.getY();
    mZ = quat.getZ();
    mW = quat.getW();
}

inline Vector4::Vector4( vec_float4 scalar )
{
    mX = scalar;
    mY = scalar;
    mZ = scalar;
    mW = scalar;
}

inline Vector4::Vector4( const Aos::Vector4 & vec )
{
    mX = vec.get128();
    mY = vec.get128();
    mZ = vec.get128();
    mW = vec.get128();
    _MM_TRANSPOSE4_PS( mX, mY, mZ, mW );
    //vec_uchar16 shuffle_xxxx = (vec_uchar16)vec_splats((int)0x00010203);
    //vec_uchar16 shuffle_yyyy = (vec_uchar16)vec_splats((int)0x04050607);
    //vec_uchar16 shuffle_zzzz = (vec_uchar16)vec_splats((int)0x08090a0b);
    //vec_uchar16 shuffle_wwww = (vec_uchar16)vec_splats((int)0x0c0d0e0f);
    //vec_float4 vec128 = vec.get128();
    //mX = spu_shuffle( vec128, vec128, shuffle_xxxx );
    //mY = spu_shuffle( vec128, vec128, shuffle_yyyy );
    //mZ = spu_shuffle( vec128, vec128, shuffle_zzzz );
    //mW = spu_shuffle( vec128, vec128, shuffle_wwww );
}

inline Vector4::Vector4( const Aos::Vector4 & vec0, const Aos::Vector4 & vec1, const Aos::Vector4 & vec2, const Aos::Vector4 & vec3 )
{
    mX = vec0.get128();
    mY = vec1.get128();
    mZ = vec2.get128();
    mW = vec3.get128();
    _MM_TRANSPOSE4_PS( mX, mY, mZ, mW );
    
    //vec_float4 tmp0, tmp1, tmp2, tmp3;
    //tmp0 = spu_shuffle( vec0.get128(), vec2.get128(), _VECTORMATH_SHUF_XAYB );
    //tmp1 = spu_shuffle( vec1.get128(), vec3.get128(), _VECTORMATH_SHUF_XAYB );
    //tmp2 = spu_shuffle( vec0.get128(), vec2.get128(), _VECTORMATH_SHUF_ZCWD );
    //tmp3 = spu_shuffle( vec1.get128(), vec3.get128(), _VECTORMATH_SHUF_ZCWD );
    //mX = spu_shuffle( tmp0, tmp1, _VECTORMATH_SHUF_XAYB );
    //mY = spu_shuffle( tmp0, tmp1, _VECTORMATH_SHUF_ZCWD );
    //mZ = spu_shuffle( tmp2, tmp3, _VECTORMATH_SHUF_XAYB );
    //mW = spu_shuffle( tmp2, tmp3, _VECTORMATH_SHUF_ZCWD );
}

inline const Vector4 Vector4::xAxis( )
{
    return Vector4( vec_splats(1.0f), vec_splats(0.0f), vec_splats(0.0f), vec_splats(0.0f) );
}

inline const Vector4 Vector4::yAxis( )
{
    return Vector4( vec_splats(0.0f), vec_splats(1.0f), vec_splats(0.0f), vec_splats(0.0f) );
}

inline const Vector4 Vector4::zAxis( )
{
    return Vector4( vec_splats(0.0f), vec_splats(0.0f), vec_splats(1.0f), vec_splats(0.0f) );
}

inline const Vector4 Vector4::wAxis( )
{
    return Vector4( vec_splats(0.0f), vec_splats(0.0f), vec_splats(0.0f), vec_splats(1.0f) );
}

inline const Vector4 lerp( vec_float4 t, const Vector4 & vec0, const Vector4 & vec1 )
{
    return ( vec0 + ( ( vec1 - vec0 ) * t ) );
}

inline const Vector4 slerp( vec_float4 t, const Vector4 & unitVec0, const Vector4 & unitVec1 )
{
    vec_float4 recipSinAngle, scale0, scale1, cosAngle, angle;
    vec_uint4 selectMask;
    cosAngle = dot( unitVec0, unitVec1 );
    selectMask = (vec_uint4)vec_cmpgt( vec_splats(_VECTORMATH_SLERP_TOL), cosAngle );
    angle = acosf4( cosAngle );
	recipSinAngle = recipf4_newtonrapson( sinf4( angle ) );
    scale0 = vec_sel( vec_sub( vec_splats(1.0f), t ), vec_mul( sinf4( vec_mul( vec_sub( vec_splats(1.0f), t ), angle ) ), recipSinAngle ), selectMask );
    scale1 = vec_sel( t, vec_mul( sinf4( vec_mul( t, angle ) ), recipSinAngle ), selectMask );
    return ( ( unitVec0 * scale0 ) + ( unitVec1 * scale1 ) );
}

inline void Vector4::get4Aos( Aos::Vector4 & result0, Aos::Vector4 & result1, Aos::Vector4 & result2, Aos::Vector4 & result3 ) const
{
    vec_float4 vec0 = mX;
    vec_float4 vec1 = mY;
    vec_float4 vec2 = mZ;
    vec_float4 vec3 = mW;
    _MM_TRANSPOSE4_PS( vec0, vec1, vec2, vec3 );

    result0 = Aos::Vector4( vec0 );
    result1 = Aos::Vector4( vec1 );
    result2 = Aos::Vector4( vec2 );
    result3 = Aos::Vector4( vec3 );

    //vec_float4 tmp0, tmp1, tmp2, tmp3;
    //tmp0 = spu_shuffle( mX, mZ, _VECTORMATH_SHUF_XAYB );
    //tmp1 = spu_shuffle( mY, mW, _VECTORMATH_SHUF_XAYB );
    //tmp2 = spu_shuffle( mX, mZ, _VECTORMATH_SHUF_ZCWD );
    //tmp3 = spu_shuffle( mY, mW, _VECTORMATH_SHUF_ZCWD );
    //result0 = Aos::Vector4( spu_shuffle( tmp0, tmp1, _VECTORMATH_SHUF_XAYB ) );
    //result1 = Aos::Vector4( spu_shuffle( tmp0, tmp1, _VECTORMATH_SHUF_ZCWD ) );
    //result2 = Aos::Vector4( spu_shuffle( tmp2, tmp3, _VECTORMATH_SHUF_XAYB ) );
    //result3 = Aos::Vector4( spu_shuffle( tmp2, tmp3, _VECTORMATH_SHUF_ZCWD ) );
}

inline void storeHalfFloats( const Vector4 & /*vec*/, vec_ushort8 * /*twoQuads*/ )
{
    //Aos::Vector4 v0, v1, v2, v3;
    //vec.get4Aos( v0, v1, v2, v3 );
    //twoQuads[0] = _vmath2VfToHalfFloats(v0.get128(), v1.get128());
    //twoQuads[1] = _vmath2VfToHalfFloats(v2.get128(), v3.get128());
}

inline Vector4 & Vector4::operator =( const Vector4 & vec )
{
    mX = vec.mX;
    mY = vec.mY;
    mZ = vec.mZ;
    mW = vec.mW;
    return *this;
}

inline Vector4 & Vector4::setXYZ( const Vector3 & vec )
{
    mX = vec.getX();
    mY = vec.getY();
    mZ = vec.getZ();
    return *this;
}

inline const Vector3 Vector4::getXYZ( ) const
{
    return Vector3( mX, mY, mZ );
}

inline Vector4 & Vector4::setX( vec_float4 _x )
{
    mX = _x;
    return *this;
}

inline vec_float4 Vector4::getX( ) const
{
    return mX;
}

inline Vector4 & Vector4::setY( vec_float4 _y )
{
    mY = _y;
    return *this;
}

inline vec_float4 Vector4::getY( ) const
{
    return mY;
}

inline Vector4 & Vector4::setZ( vec_float4 _z )
{
    mZ = _z;
    return *this;
}

inline vec_float4 Vector4::getZ( ) const
{
    return mZ;
}

inline Vector4 & Vector4::setW( vec_float4 _w )
{
    mW = _w;
    return *this;
}

inline vec_float4 Vector4::getW( ) const
{
    return mW;
}

inline Vector4 & Vector4::setElem( int idx, vec_float4 value )
{
    *(&mX + idx) = value;
    return *this;
}

inline vec_float4 Vector4::getElem( int idx ) const
{
    return *(&mX + idx);
}

inline Vector4::vec_float4_t & Vector4::operator []( int idx )
{
    return *(&mX + idx);
}

inline vec_float4 Vector4::operator []( int idx ) const
{
    return *(&mX + idx);
}

inline const Vector4 Vector4::operator +( const Vector4 & vec ) const
{
    return Vector4(
        vec_add( mX, vec.mX ),
        vec_add( mY, vec.mY ),
        vec_add( mZ, vec.mZ ),
        vec_add( mW, vec.mW )
    );
}

inline const Vector4 Vector4::operator -( const Vector4 & vec ) const
{
    return Vector4(
        vec_sub( mX, vec.mX ),
        vec_sub( mY, vec.mY ),
        vec_sub( mZ, vec.mZ ),
        vec_sub( mW, vec.mW )
    );
}

inline const Vector4 Vector4::operator *( vec_float4 scalar ) const
{
    return Vector4(
        vec_mul( mX, scalar ),
        vec_mul( mY, scalar ),
        vec_mul( mZ, scalar ),
        vec_mul( mW, scalar )
    );
}

inline Vector4 & Vector4::operator +=( const Vector4 & vec )
{
    *this = *this + vec;
    return *this;
}

inline Vector4 & Vector4::operator -=( const Vector4 & vec )
{
    *this = *this - vec;
    return *this;
}

inline Vector4 & Vector4::operator *=( vec_float4 scalar )
{
    *this = *this * scalar;
    return *this;
}

inline const Vector4 Vector4::operator /( vec_float4 scalar ) const
{
    return Vector4(
        vec_div( mX, scalar ),
        vec_div( mY, scalar ),
        vec_div( mZ, scalar ),
        vec_div( mW, scalar )
    );
}

inline Vector4 & Vector4::operator /=( vec_float4 scalar )
{
    *this = *this / scalar;
    return *this;
}

inline const Vector4 Vector4::operator -( ) const
{
    return Vector4(
        negatef4( mX ),
        negatef4( mY ),
        negatef4( mZ ),
        negatef4( mW )
    );
}

inline const Vector4 operator *( vec_float4 scalar, const Vector4 & vec )
{
    return vec * scalar;
}

inline const Vector4 mulPerElem( const Vector4 & vec0, const Vector4 & vec1 )
{
    return Vector4(
        vec_mul( vec0.getX(), vec1.getX() ),
        vec_mul( vec0.getY(), vec1.getY() ),
        vec_mul( vec0.getZ(), vec1.getZ() ),
        vec_mul( vec0.getW(), vec1.getW() )
    );
}

inline const Vector4 divPerElem( const Vector4 & vec0, const Vector4 & vec1 )
{
    return Vector4(
        vec_div( vec0.getX(), vec1.getX() ),
        vec_div( vec0.getY(), vec1.getY() ),
        vec_div( vec0.getZ(), vec1.getZ() ),
        vec_div( vec0.getW(), vec1.getW() )
    );
}

inline const Vector4 recipPerElem( const Vector4 & vec )
{
    return Vector4(
        recipf4( vec.getX() ),
        recipf4( vec.getY() ),
        recipf4( vec.getZ() ),
        recipf4( vec.getW() )
    );
}

inline const Vector4 sqrtPerElem( const Vector4 & vec )
{
    return Vector4(
        sqrtf4( vec.getX() ),
        sqrtf4( vec.getY() ),
        sqrtf4( vec.getZ() ),
        sqrtf4( vec.getW() )
    );
}

inline const Vector4 rsqrtPerElem( const Vector4 & vec )
{
    return Vector4(
        rsqrtf4( vec.getX() ),
        rsqrtf4( vec.getY() ),
        rsqrtf4( vec.getZ() ),
        rsqrtf4( vec.getW() )
    );
}

inline const Vector4 absPerElem( const Vector4 & vec )
{
    return Vector4(
        fabsf4( vec.getX() ),
        fabsf4( vec.getY() ),
        fabsf4( vec.getZ() ),
        fabsf4( vec.getW() )
    );
}

inline const Vector4 copySignPerElem( const Vector4 & vec0, const Vector4 & vec1 )
{
    return Vector4(
        copysignf4( vec0.getX(), vec1.getX() ),
        copysignf4( vec0.getY(), vec1.getY() ),
        copysignf4( vec0.getZ(), vec1.getZ() ),
        copysignf4( vec0.getW(), vec1.getW() )
    );
}

inline const Vector4 maxPerElem( const Vector4 & vec0, const Vector4 & vec1 )
{
    return Vector4(
        vec_max( vec0.getX(), vec1.getX() ),
        vec_max( vec0.getY(), vec1.getY() ),
        vec_max( vec0.getZ(), vec1.getZ() ),
        vec_max( vec0.getW(), vec1.getW() )
    );
}

inline vec_float4 maxElem( const Vector4 & vec )
{
    vec_float4 result;
    result = vec_max( vec.getX(), vec.getY() );
    result = vec_max( vec.getZ(), result );
    result = vec_max( vec.getW(), result );
    return result;
}

inline const Vector4 minPerElem( const Vector4 & vec0, const Vector4 & vec1 )
{
    return Vector4(
        vec_min( vec0.getX(), vec1.getX() ),
        vec_min( vec0.getY(), vec1.getY() ),
        vec_min( vec0.getZ(), vec1.getZ() ),
        vec_min( vec0.getW(), vec1.getW() )
    );
}

inline vec_float4 minElem( const Vector4 & vec )
{
    vec_float4 result;
    result = vec_min( vec.getX(), vec.getY() );
    result = vec_min( vec.getZ(), result );
    result = vec_min( vec.getW(), result );
    return result;
}

inline vec_float4 sum( const Vector4 & vec )
{
    vec_float4 result;
    result = vec_add( vec.getX(), vec.getY() );
    result = vec_add( result, vec.getZ() );
    result = vec_add( result, vec.getW() );
    return result;
}

inline vec_float4 dot( const Vector4 & vec0, const Vector4 & vec1 )
{
    vec_float4 result;
    result = vec_mul( vec0.getX(), vec1.getX() );
    result = vec_add( result, vec_mul( vec0.getY(), vec1.getY() ) );
    result = vec_add( result, vec_mul( vec0.getZ(), vec1.getZ() ) );
    result = vec_add( result, vec_mul( vec0.getW(), vec1.getW() ) );
    return result;
}

inline vec_float4 lengthSqr( const Vector4 & vec )
{
    vec_float4 result;
    result = vec_mul( vec.getX(), vec.getX() );
    result = vec_add( result, vec_mul( vec.getY(), vec.getY() ) );
    result = vec_add( result, vec_mul( vec.getZ(), vec.getZ() ) );
    result = vec_add( result, vec_mul( vec.getW(), vec.getW() ) );
    return result;
}

inline vec_float4 length( const Vector4 & vec )
{
    return sqrtf4( lengthSqr( vec ) );
}

inline const Vector4 normalize( const Vector4 & vec )
{
    vec_float4 lenSqr, lenInv;
    lenSqr = lengthSqr( vec );
	lenInv = newtonrapson_rsqrt4( lenSqr );
    return Vector4(
        vec_mul( vec.getX(), lenInv ),
        vec_mul( vec.getY(), lenInv ),
        vec_mul( vec.getZ(), lenInv ),
        vec_mul( vec.getW(), lenInv )
    );
}

inline const Vector4 select( const Vector4 & vec0, const Vector4 & vec1, vec_uint4 select1 )
{
    return Vector4(
        vec_sel( vec0.getX(), vec1.getX(), select1 ),
        vec_sel( vec0.getY(), vec1.getY(), select1 ),
        vec_sel( vec0.getZ(), vec1.getZ(), select1 ),
        vec_sel( vec0.getW(), vec1.getW(), select1 )
    );
}

#ifdef _VECTORMATH_DEBUG

inline void print( const Vector4 & vec )
{
    Aos::Vector4 vec0, vec1, vec2, vec3;
    vec.get4Aos( vec0, vec1, vec2, vec3 );
    printf("slot 0:\n");
    print( vec0 );
    printf("slot 1:\n");
    print( vec1 );
    printf("slot 2:\n");
    print( vec2 );
    printf("slot 3:\n");
    print( vec3 );
}

inline void print( const Vector4 & vec, const char * name )
{
    Aos::Vector4 vec0, vec1, vec2, vec3;
    printf( "%s:\n", name );
    vec.get4Aos( vec0, vec1, vec2, vec3 );
    printf("slot 0:\n");
    print( vec0 );
    printf("slot 1:\n");
    print( vec1 );
    printf("slot 2:\n");
    print( vec2 );
    printf("slot 3:\n");
    print( vec3 );
}

#endif

inline Point3::Point3( const Point3 & pnt )
{
    mX = pnt.mX;
    mY = pnt.mY;
    mZ = pnt.mZ;
}

inline Point3::Point3( vec_float4 _x, vec_float4 _y, vec_float4 _z )
{
    mX = _x;
    mY = _y;
    mZ = _z;
}

inline Point3::Point3( const Vector3 & vec )
{
    mX = vec.getX();
    mY = vec.getY();
    mZ = vec.getZ();
}

inline Point3::Point3( vec_float4 scalar )
{
    mX = scalar;
    mY = scalar;
    mZ = scalar;
}

inline Point3::Point3( const Aos::Point3 & pnt )
{
    mX = pnt.get128();
    mY = pnt.get128();
    mZ = pnt.get128();
    vec_float4 tmp = pnt.get128();
    _MM_TRANSPOSE4_PS( mX, mY, mZ, tmp );
//     vec_uchar16 shuffle_xxxx = (vec_uchar16)vec_splats((int)0x00010203);
//     vec_uchar16 shuffle_yyyy = (vec_uchar16)vec_splats((int)0x04050607);
//     vec_uchar16 shuffle_zzzz = (vec_uchar16)vec_splats((int)0x08090a0b);
//     vec_float4 vec128 = pnt.get128();
//     mX = spu_shuffle( vec128, vec128, shuffle_xxxx );
//     mY = spu_shuffle( vec128, vec128, shuffle_yyyy );
//     mZ = spu_shuffle( vec128, vec128, shuffle_zzzz );
}

inline Point3::Point3( const Aos::Point3 & pnt0, const Aos::Point3 & pnt1, const Aos::Point3 & pnt2, const Aos::Point3 & pnt3 )
{
    mX = pnt0.get128();
    mY = pnt1.get128();
    mZ = pnt2.get128();
    vec_float4 tmp = pnt3.get128();
    _MM_TRANSPOSE4_PS( mX, mY, mZ, tmp );
    
    //vec_float4 tmp0, tmp1, tmp2, tmp3;
    //tmp0 = spu_shuffle( pnt0.get128(), pnt2.get128(), _VECTORMATH_SHUF_XAYB );
    //tmp1 = spu_shuffle( pnt1.get128(), pnt3.get128(), _VECTORMATH_SHUF_XAYB );
    //tmp2 = spu_shuffle( pnt0.get128(), pnt2.get128(), _VECTORMATH_SHUF_ZCWD );
    //tmp3 = spu_shuffle( pnt1.get128(), pnt3.get128(), _VECTORMATH_SHUF_ZCWD );
    //mX = spu_shuffle( tmp0, tmp1, _VECTORMATH_SHUF_XAYB );
    //mY = spu_shuffle( tmp0, tmp1, _VECTORMATH_SHUF_ZCWD );
    //mZ = spu_shuffle( tmp2, tmp3, _VECTORMATH_SHUF_XAYB );
}

inline const Point3 lerp( vec_float4 t, const Point3 & pnt0, const Point3 & pnt1 )
{
    return ( pnt0 + ( ( pnt1 - pnt0 ) * t ) );
}

inline void Point3::get4Aos( Aos::Point3 & result0, Aos::Point3 & result1, Aos::Point3 & result2, Aos::Point3 & result3 ) const
{
    vec_float4 vec0 = mX;
    vec_float4 vec1 = mY;
    vec_float4 vec2 = mZ;
    vec_float4 vec3 = { 1.f, 1.f, 1.f, 1.f };
    _MM_TRANSPOSE4_PS( vec0, vec1, vec2, vec3 );

    result0 = Aos::Point3( vec0 );
    result1 = Aos::Point3( vec1 );
    result2 = Aos::Point3( vec2 );
    result3 = Aos::Point3( vec3 );

    //vec_float4 tmp0, tmp1;
    //tmp0 = spu_shuffle( mX, mZ, _VECTORMATH_SHUF_XAYB );
    //tmp1 = spu_shuffle( mX, mZ, _VECTORMATH_SHUF_ZCWD );
    //result0 = Aos::Point3( spu_shuffle( tmp0, mY, _VECTORMATH_SHUF_XAYB ) );
    //result1 = Aos::Point3( spu_shuffle( tmp0, mY, _VECTORMATH_SHUF_ZBW0 ) );
    //result2 = Aos::Point3( spu_shuffle( tmp1, mY, _VECTORMATH_SHUF_XCY0 ) );
    //result3 = Aos::Point3( spu_shuffle( tmp1, mY, _VECTORMATH_SHUF_ZDW0 ) );
}

inline void loadXYZArray( Point3 & /*vec*/, const vec_float4 * /*threeQuads*/ )
{
//     vec_float4 xyxy, yzyz, zxzx, xyzx, yzxy, zxyz;
//     xyzx = threeQuads[0];
//     yzxy = threeQuads[1];
//     zxyz = threeQuads[2];
//     xyxy = spu_shuffle( xyzx, yzxy, _VECTORMATH_SHUF_XYCD );
//     zxzx = spu_shuffle( zxyz, xyzx, _VECTORMATH_SHUF_XYCD );
//     yzyz = spu_shuffle( yzxy, zxyz, _VECTORMATH_SHUF_XYCD );
//     vec.setX( spu_shuffle( xyxy, zxzx, _VECTORMATH_SHUF_XDZB ) );
//     vec.setY( spu_shuffle( xyxy, yzyz, _VECTORMATH_SHUF_YAWC ) );
//     vec.setZ( spu_shuffle( zxzx, yzyz, _VECTORMATH_SHUF_ZBXD ) );
}

inline void storeXYZArray( const Point3 & /*vec*/, vec_float4 * /*threeQuads*/ )
{
//     vec_float4 xyzx, yzxy, zxyz, xyxy, zxzx, yzyz;
//     xyxy = spu_shuffle( vec.getX(), vec.getY(), _VECTORMATH_SHUF_XAZC );
//     zxzx = spu_shuffle( vec.getZ(), vec.getX(), _VECTORMATH_SHUF_ZDXB );
//     yzyz = spu_shuffle( vec.getY(), vec.getZ(), _VECTORMATH_SHUF_YBWD );
//     xyzx = spu_shuffle( xyxy, zxzx, _VECTORMATH_SHUF_XYCD );
//     yzxy = spu_shuffle( yzyz, xyxy, _VECTORMATH_SHUF_XYCD );
//     zxyz = spu_shuffle( zxzx, yzyz, _VECTORMATH_SHUF_XYCD );
//     threeQuads[0] = xyzx;
//     threeQuads[1] = yzxy;
//     threeQuads[2] = zxyz;
}

inline void storeHalfFloats( const Point3 & /*pnt0*/, const Point3 & /*pnt1*/, vec_ushort8 * /*threeQuads*/ )
{
//     vec_float4 xyz0[3];
//     vec_float4 xyz1[3];
//     storeXYZArray( pnt0, xyz0 );
//     storeXYZArray( pnt1, xyz1 );
//     threeQuads[0] = _vmath2VfToHalfFloats(xyz0[0], xyz0[1]);
//     threeQuads[1] = _vmath2VfToHalfFloats(xyz0[2], xyz1[0]);
//     threeQuads[2] = _vmath2VfToHalfFloats(xyz1[1], xyz1[2]);
}

inline Point3 & Point3::operator =( const Point3 & pnt )
{
    mX = pnt.mX;
    mY = pnt.mY;
    mZ = pnt.mZ;
    return *this;
}

inline Point3 & Point3::setX( vec_float4 _x )
{
    mX = _x;
    return *this;
}

inline vec_float4 Point3::getX( ) const
{
    return mX;
}

inline Point3 & Point3::setY( vec_float4 _y )
{
    mY = _y;
    return *this;
}

inline vec_float4 Point3::getY( ) const
{
    return mY;
}

inline Point3 & Point3::setZ( vec_float4 _z )
{
    mZ = _z;
    return *this;
}

inline vec_float4 Point3::getZ( ) const
{
    return mZ;
}

inline Point3 & Point3::setElem( int idx, vec_float4 value )
{
    *(&mX + idx) = value;
    return *this;
}

inline vec_float4 Point3::getElem( int idx ) const
{
    return *(&mX + idx);
}

inline Point3::vec_float4_t & Point3::operator []( int idx )
{
    return *(&mX + idx);
}

inline vec_float4 Point3::operator []( int idx ) const
{
    return *(&mX + idx);
}

inline const Vector3 Point3::operator -( const Point3 & pnt ) const
{
    return Vector3(
        vec_sub( mX, pnt.mX ),
        vec_sub( mY, pnt.mY ),
        vec_sub( mZ, pnt.mZ )
    );
}

inline const Point3 Point3::operator +( const Vector3 & vec ) const
{
    return Point3(
        vec_add( mX, vec.getX() ),
        vec_add( mY, vec.getY() ),
        vec_add( mZ, vec.getZ() )
    );
}

inline const Point3 Point3::operator -( const Vector3 & vec ) const
{
    return Point3(
        vec_sub( mX, vec.getX() ),
        vec_sub( mY, vec.getY() ),
        vec_sub( mZ, vec.getZ() )
    );
}

inline Point3 & Point3::operator +=( const Vector3 & vec )
{
    *this = *this + vec;
    return *this;
}

inline Point3 & Point3::operator -=( const Vector3 & vec )
{
    *this = *this - vec;
    return *this;
}

inline const Point3 mulPerElem( const Point3 & pnt0, const Point3 & pnt1 )
{
    return Point3(
        vec_mul( pnt0.getX(), pnt1.getX() ),
        vec_mul( pnt0.getY(), pnt1.getY() ),
        vec_mul( pnt0.getZ(), pnt1.getZ() )
    );
}

inline const Point3 divPerElem( const Point3 & pnt0, const Point3 & pnt1 )
{
    return Point3(
        vec_div( pnt0.getX(), pnt1.getX() ),
        vec_div( pnt0.getY(), pnt1.getY() ),
        vec_div( pnt0.getZ(), pnt1.getZ() )
    );
}

inline const Point3 recipPerElem( const Point3 & pnt )
{
    return Point3(
        recipf4( pnt.getX() ),
        recipf4( pnt.getY() ),
        recipf4( pnt.getZ() )
    );
}

inline const Point3 sqrtPerElem( const Point3 & pnt )
{
    return Point3(
        sqrtf4( pnt.getX() ),
        sqrtf4( pnt.getY() ),
        sqrtf4( pnt.getZ() )
    );
}

inline const Point3 rsqrtPerElem( const Point3 & pnt )
{
    return Point3(
        rsqrtf4( pnt.getX() ),
        rsqrtf4( pnt.getY() ),
        rsqrtf4( pnt.getZ() )
    );
}

inline const Point3 absPerElem( const Point3 & pnt )
{
    return Point3(
        fabsf4( pnt.getX() ),
        fabsf4( pnt.getY() ),
        fabsf4( pnt.getZ() )
    );
}

inline const Point3 copySignPerElem( const Point3 & pnt0, const Point3 & pnt1 )
{
    return Point3(
        copysignf4( pnt0.getX(), pnt1.getX() ),
        copysignf4( pnt0.getY(), pnt1.getY() ),
        copysignf4( pnt0.getZ(), pnt1.getZ() )
    );
}

inline const Point3 maxPerElem( const Point3 & pnt0, const Point3 & pnt1 )
{
    return Point3(
        vec_max( pnt0.getX(), pnt1.getX() ),
        vec_max( pnt0.getY(), pnt1.getY() ),
        vec_max( pnt0.getZ(), pnt1.getZ() )
    );
}

inline vec_float4 maxElem( const Point3 & pnt )
{
    vec_float4 result;
    result = vec_max( pnt.getX(), pnt.getY() );
    result = vec_max( pnt.getZ(), result );
    return result;
}

inline const Point3 minPerElem( const Point3 & pnt0, const Point3 & pnt1 )
{
    return Point3(
        vec_min( pnt0.getX(), pnt1.getX() ),
        vec_min( pnt0.getY(), pnt1.getY() ),
        vec_min( pnt0.getZ(), pnt1.getZ() )
    );
}

inline vec_float4 minElem( const Point3 & pnt )
{
    vec_float4 result;
    result = vec_min( pnt.getX(), pnt.getY() );
    result = vec_min( pnt.getZ(), result );
    return result;
}

inline vec_float4 sum( const Point3 & pnt )
{
    vec_float4 result;
    result = vec_add( pnt.getX(), pnt.getY() );
    result = vec_add( result, pnt.getZ() );
    return result;
}

inline const Point3 scale( const Point3 & pnt, vec_float4 scaleVal )
{
    return mulPerElem( pnt, Point3( scaleVal ) );
}

inline const Point3 scale( const Point3 & pnt, const Vector3 & scaleVec )
{
    return mulPerElem( pnt, Point3( scaleVec ) );
}

inline vec_float4 projection( const Point3 & pnt, const Vector3 & unitVec )
{
    vec_float4 result;
    result = vec_mul( pnt.getX(), unitVec.getX() );
    result = vec_add( result, vec_mul( pnt.getY(), unitVec.getY() ) );
    result = vec_add( result, vec_mul( pnt.getZ(), unitVec.getZ() ) );
    return result;
}

inline vec_float4 distSqrFromOrigin( const Point3 & pnt )
{
    return lengthSqr( Vector3( pnt ) );
}

inline vec_float4 distFromOrigin( const Point3 & pnt )
{
    return length( Vector3( pnt ) );
}

inline vec_float4 distSqr( const Point3 & pnt0, const Point3 & pnt1 )
{
    return lengthSqr( ( pnt1 - pnt0 ) );
}

inline vec_float4 dist( const Point3 & pnt0, const Point3 & pnt1 )
{
    return length( ( pnt1 - pnt0 ) );
}

inline const Point3 select( const Point3 & pnt0, const Point3 & pnt1, vec_uint4 select1 )
{
    return Point3(
        vec_sel( pnt0.getX(), pnt1.getX(), select1 ),
        vec_sel( pnt0.getY(), pnt1.getY(), select1 ),
        vec_sel( pnt0.getZ(), pnt1.getZ(), select1 )
    );
}

#ifdef _VECTORMATH_DEBUG

inline void print( const Point3 & pnt )
{
    Aos::Point3 vec0, vec1, vec2, vec3;
    pnt.get4Aos( vec0, vec1, vec2, vec3 );
    printf("slot 0:\n");
    print( vec0 );
    printf("slot 1:\n");
    print( vec1 );
    printf("slot 2:\n");
    print( vec2 );
    printf("slot 3:\n");
    print( vec3 );
}

inline void print( const Point3 & pnt, const char * name )
{
    Aos::Point3 vec0, vec1, vec2, vec3;
    printf( "%s:\n", name );
    pnt.get4Aos( vec0, vec1, vec2, vec3 );
    printf("slot 0:\n");
    print( vec0 );
    printf("slot 1:\n");
    print( vec1 );
    printf("slot 2:\n");
    print( vec2 );
    printf("slot 3:\n");
    print( vec3 );
}

#endif

} // namespace Soa
} // namespace Vectormath

#endif
