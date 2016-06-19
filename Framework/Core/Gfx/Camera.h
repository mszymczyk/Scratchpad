#pragma once

#include <Util\vectormath.h>

namespace spad
{

// Abstract base class for camera types
class Camera
{

protected:

    Matrix4 view;
    Matrix4 projection;
    Matrix4 viewProjection;

    Matrix4 world;
    Vector3 position;
    Quat orientation;

    float nearZ;
    float farZ;

    virtual void CreateProjection() = 0;
    void WorldMatrixChanged();

public:

    Camera(float nearZ, float farZ);
    ~Camera();

    const Matrix4& ViewMatrix() const { return view; };
    const Matrix4& ProjectionMatrix() const { return projection; };
    const Matrix4& ViewProjectionMatrix() const { return viewProjection; };
    const Matrix4& WorldMatrix() const { return world; };
    const Vector3& Position() const { return position; };
	const Quat& Orientation() const { return orientation; };
    float NearClip() const { return nearZ; };
    float FarClip() const { return farZ; };

	Vector3 Forward() const
	{
		return -world.getCol2().getXYZ();
	}
	Vector3 Back() const
	{
		return world.getCol2().getXYZ();
	}
	Vector3 Up() const
	{
		return world.getCol1().getXYZ();
	}
	Vector3 Down() const
	{
		return -world.getCol1().getXYZ();
	}
	Vector3 Right() const
	{
		return world.getCol0().getXYZ();
	}
	Vector3 Left() const
	{
		return -world.getCol0().getXYZ();
	}

    void SetLookAt(const Vector3& eye, const Vector3& lookAt, const Vector3& up);
    void SetWorldMatrix(const Matrix4& newWorld);
    void SetPosition(const Vector3& newPosition);
	void SetOrientation( const Quat& newOrientation );
    void SetNearClip(float newNearClip);
    void SetFarClip(float newFarClip);
    void SetProjection(const Matrix4& newProjection);
};

// Camera with an orthographic projection
class OrthographicCamera : public Camera
{

protected:

    float xMin;
    float xMax;
    float yMin;
    float yMax;

    virtual void CreateProjection();

public:

    OrthographicCamera(float minX, float minY, float maxX, float maxY, float nearClip, float farClip);
    ~OrthographicCamera();

    float MinX() const { return xMin; };
    float MinY() const { return yMin; };
    float MaxX() const { return xMax; };
    float MaxY() const { return yMax; };

    void SetMinX(float minX);
    void SetMinY(float minY);
    void SetMaxX(float maxX);
    void SetMaxY(float maxY);
};

// Camera with a perspective projection
class PerspectiveCamera : public Camera
{

protected:

    float aspect;
    float fov;

    virtual void CreateProjection();

public:

    PerspectiveCamera(float aspectRatio, float fieldOfView, float nearClip, float farClip);
    ~PerspectiveCamera();

    float AspectRatio() const { return aspect; };
    float FieldOfView() const { return fov; };

    void SetAspectRatio(float aspectRatio);
    void SetFieldOfView(float fieldOfView);
};

// Perspective camera that rotates about Z and Y axes
class FirstPersonCamera : public PerspectiveCamera
{

protected:

    float xRot;
    float yRot;

public:

    FirstPersonCamera(float aspectRatio, float fieldOfView, float nearClip, float farClip);
    ~FirstPersonCamera();

    float XRotation() const { return xRot; };
    float YRotation() const { return yRot; };

    void SetXRotation(float xRotation);
    void SetYRotation(float yRotation);
};

}