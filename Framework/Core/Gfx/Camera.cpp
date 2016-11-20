#include "Gfx_pch.h"
#include "Camera.h"
#include "Util\Bits.h"

namespace spad
{

//=================================================================================================
// Camera
//=================================================================================================
Camera::Camera(float nearClip, float farClip) : nearZ(nearClip),
                                                farZ(farClip)
{
    SPAD_ASSERT(nearZ >= 0.0f && nearZ < farZ);
    SPAD_ASSERT(farZ >= 0.0f && farZ > nearZ);

    world = Matrix4::identity();
    view = Matrix4::identity();
    position = Vector3(0.0f, 0.0f, 0.0f);
    orientation = Quat::identity();
}

Camera::~Camera()
{

}

void Camera::WorldMatrixChanged()
{
    view = affineInverse(world);
	viewProjection = projection * view;
}

//Vector3 Camera::Forward() const
//{
//    return world.Forward();
//}
//
//Vector3 Camera::Back() const
//{
//    return world.Back();
//}
//
//Vector3 Camera::Up() const
//{
//    return world.Up();
//}
//
//Vector3 Camera::Down() const
//{
//    return world.Down();
//}
//
//Vector3 Camera::Right() const
//{
//    return world.Right();
//}
//
//Vector3 Camera::Left() const
//{
//    return world.Left();
//}

void Camera::SetLookAt(const Vector3 &eye, const Vector3 &lookAt, const Vector3 &up)
{
    //view = XMMatrixLookAtLH(eye.ToSIMD(), lookAt.ToSIMD(), up.ToSIMD());
    //world = Matrix4::Invert(view);
	view = Matrix4::lookAt( Point3(eye), Point3(lookAt), up );
	world = affineInverse( view );
    position = eye;
    //orientation = XMQuaternionRotationMatrix(world.ToSIMD());
	orientation = Quat( world.getUpper3x3() );

    WorldMatrixChanged();
}

void Camera::SetWorldMatrix(const Matrix4& newWorld)
{
    world = newWorld;
    position = world.getTranslation();
    //orientation = XMQuaternionRotationMatrix(world.ToSIMD());
	orientation = Quat( world.getUpper3x3() );

    WorldMatrixChanged();
}

void Camera::SetPosition(const Vector3& newPosition)
{
    position = newPosition;
    //world.SetTranslation(newPosition);
	world.setTranslation( newPosition );

    WorldMatrixChanged();
}

void Camera::SetOrientation(const Quat& newOrientation)
{
    //world = XMMatrixRotationQuaternion(newOrientation.ToSIMD());
    orientation = newOrientation;
    //world.SetTranslation(position);
	world.setUpper3x3( Matrix3( newOrientation ) );

    WorldMatrixChanged();
}

void Camera::SetNearClip(float newNearClip)
{
    nearZ = newNearClip;
    CreateProjection();
}

void Camera::SetFarClip(float newFarClip)
{
    farZ = newFarClip;
    CreateProjection();
}

void Camera::SetProjection(const Matrix4& newProjection)
{
    projection = newProjection;
	viewProjection = projection * view;
}

//=================================================================================================
// OrthographicCamera
//=================================================================================================

OrthographicCamera::OrthographicCamera(float minX, float minY, float maxX,
                                       float maxY, float nearClip, float farClip) : Camera(nearClip, farClip),
                                                                                    xMin(minX),
                                                                                    yMin(minY),
                                                                                    xMax(maxX),
                                                                                    yMax(maxY)

{
    SPAD_ASSERT(xMax > xMin && yMax > yMin);

    CreateProjection();
}

OrthographicCamera::~OrthographicCamera()
{

}

void OrthographicCamera::CreateProjection()
{
    //projection = XMMatrixOrthographicOffCenterLH(xMin, xMax, yMin, yMax, nearZ, farZ);
	projection = Matrix4::orthographic( xMin, xMax, yMin, yMax, nearZ, farZ );
	// Matrix4::ortographic creates OpenGL style matrix, with z mapping to <-1,1>
	// Apply scale and translation to map z to <0,1>
	Matrix4 sc = Matrix4::scale( Vector3( 1, 1, 0.5f ) );
	Matrix4 tr = Matrix4::translation( Vector3( 0, 0, 1 ) );
	projection = sc * tr * projection;

	viewProjection = projection * view;
}

void OrthographicCamera::SetMinX(float minX)
{
    xMin = minX;
    CreateProjection();
}

void OrthographicCamera::SetMinY(float minY)
{
    yMin = minY;
    CreateProjection();
}

void OrthographicCamera::SetMaxX(float maxX)
{
    xMax = maxX;
    CreateProjection();
}

void OrthographicCamera::SetMaxY(float maxY)
{
    yMax = maxY;
    CreateProjection();
}

//=================================================================================================
// PerspectiveCamera
//=================================================================================================

PerspectiveCamera::PerspectiveCamera(float aspectRatio, float fieldOfView,
                                     float nearClip, float farClip) :   Camera(nearClip, farClip),
                                                                        aspect(aspectRatio),
                                                                        fov(fieldOfView)
{
    SPAD_ASSERT(aspectRatio > 0);
    SPAD_ASSERT(fieldOfView > 0 && fieldOfView <= 3.14159f);
    CreateProjection();
}

PerspectiveCamera::~PerspectiveCamera()
{

}

void PerspectiveCamera::SetAspectRatio(float aspectRatio)
{
    aspect = aspectRatio;
    CreateProjection();
}

void PerspectiveCamera::SetFieldOfView(float fieldOfView)
{
    fov = fieldOfView;
    CreateProjection();
}

void PerspectiveCamera::CreateProjection()
{
    //projection = XMMatrixPerspectiveFovLH(fov, aspect, nearZ, farZ);
	projection = Matrix4::perspective( fov, aspect, nearZ, farZ );
	// Matrix4::perspective creates OpenGL style matrix, with z mapping to <-1,1>
	// Apply scale and translation to map z to <0,1>
	Matrix4 sc = Matrix4::scale( Vector3( 1, 1, 0.5f ) );
	Matrix4 tr = Matrix4::translation( Vector3( 0, 0, 1 ) );
	projection = sc * tr * projection;

	viewProjection = projection * view;
}

//=================================================================================================
// FirstPersonCamera
//=================================================================================================

FirstPersonCamera::FirstPersonCamera(float aspectRatio, float fieldOfView,
                                     float nearClip, float farClip) : PerspectiveCamera(aspectRatio, fieldOfView,
                                                                                        nearClip, farClip),
                                                                                        xRot(0),
                                                                                        yRot(0)
{

}

FirstPersonCamera::~FirstPersonCamera()
{

}

void FirstPersonCamera::SetXRotation(float xRotation)
{
    xRot = clamp(xRotation, -XM_PIDIV2, XM_PIDIV2);
    //SetOrientation(XMQuaternionRotationRollPitchYaw(xRot, yRot, 0));
	SetOrientation( Quat::rotationY( yRot ) * Quat::rotationX( xRot ) );
}

void FirstPersonCamera::SetYRotation(float yRotation)
{
    yRot = XMScalarModAngle(yRotation);
    //SetOrientation(XMQuaternionRotationRollPitchYaw(xRot, yRot, 0));
	SetOrientation( Quat::rotationY( yRot ) * Quat::rotationX( xRot ) );
}

} // namespace spad
