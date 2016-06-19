#pragma once

#include <Dx11Util\DxPointers.h>
#include <Gfx\Math\DirectXMathWrap.h>

namespace spad
{
	using namespace dxmath;

	class SDKMesh;

	struct MeshMaterial
	{
		Float3 AmbientAlbedo;
		Float3 DiffuseAlbedo;
		Float3 SpecularAlbedo;
		Float3 Emissive;
		float SpecularPower;
		float Alpha;
		std::string DiffuseMapName;
		std::string NormalMapName;
		ID3D11ShaderResourceViewPtr DiffuseMap;
		ID3D11ShaderResourceViewPtr NormalMap;

		MeshMaterial() : SpecularPower( 1.0f ), Alpha( 1.0f )
		{
		}
	};

	struct MeshPart
	{
		uint32_t VertexStart;
		uint32_t VertexCount;
		uint32_t IndexStart;
		uint32_t IndexCount;
		uint32_t MaterialIdx;

		MeshPart() : VertexStart( 0 ), VertexCount( 0 ), IndexStart( 0 ), IndexCount( 0 ), MaterialIdx( 0 )
		{
		}
	};

	class Mesh
	{
		friend class Model;

	public:

		enum IndexType
		{
			Index16Bit = 0,
			Index32Bit = 1
		};

		// Lifetime
		Mesh();
		~Mesh();

		// Init from loaded files
		void Initialize( ID3D11Device* device, SDKMesh& sdkmesh, uint32_t meshIdx, bool generateTangents );

		// Procedural generation
		void InitBox(ID3D11Device* device, const Float3& dimensions, const Float3& position,
		             const Quaternion& orientation, uint32_t materialIdx);

		//void InitPlane(ID3D11Device* device, const Float2& dimensions, const Float3& position,
		//               const Quaternion& orientation, uint32_t materialIdx);

		// Rendering
		void Render( ID3D11DeviceContext* context );

		// Accessors
		ID3D11Buffer* VertexBuffer() { return vertexBuffer; }
		const ID3D11Buffer* VertexBuffer() const { return vertexBuffer; }
		ID3D11Buffer* IndexBuffer() { return indexBuffer; }
		const ID3D11Buffer* IndexBuffer() const { return indexBuffer; }

		std::vector<MeshPart>& MeshParts() { return meshParts; }
		const std::vector<MeshPart>& MeshParts() const { return meshParts; }

		const D3D11_INPUT_ELEMENT_DESC* InputElements() const { return &inputElements[0]; }
		uint32_t NumInputElements() const { return static_cast<uint32_t>( inputElements.size() ); }
		uint32_t InputElementsHash() const { return inputElementsHash; }

		uint32_t VertexStride() const { return vertexStride; }
		uint32_t NumVertices() const { return numVertices; }
		uint32_t NumIndices() const { return numIndices; }

		IndexType IndexBufferType() const { return indexType; }
		DXGI_FORMAT IndexBufferFormat() const { return indexType == Index32Bit ? DXGI_FORMAT_R32_UINT : DXGI_FORMAT_R16_UINT; }
		uint32_t IndexSize() const { return indexType == Index32Bit ? 4 : 2; }

		const uint8_t* Vertices() const { return vertices.data(); }
		const uint8_t* Indices() const { return indices.data(); }

	protected:

		void GenerateTangentFrame();
		void CreateInputElements( const D3DVERTEXELEMENT9* declaration );

		ID3D11BufferPtr vertexBuffer;
		ID3D11BufferPtr indexBuffer;

		std::vector<MeshPart> meshParts;
		std::vector<D3D11_INPUT_ELEMENT_DESC> inputElements;
		uint32_t inputElementsHash;

		uint32_t vertexStride;
		uint32_t numVertices;
		uint32_t numIndices;

		IndexType indexType;

		std::vector<std::string> inputElementNames;

		std::vector<uint8_t> vertices;
		std::vector<uint8_t> indices;
	};

	class Model
	{
	public:

		// Constructor/Destructor
		Model();
		~Model();

		// Loading from file formats
		void CreateFromSDKMeshFile( ID3D11Device* device, LPCSTR fileName,
			const char* normalMapSuffix = NULL,
			bool generateTangentFrame = false,
			bool overrideNormalMaps = false );

		// Procedural generation
		void GenerateBoxScene(ID3D11Device* device);
		//void GeneratePlaneScene(ID3D11Device* device, const Float2& dimensions, const Float3& position,
		//                        const Quaternion& orientation);

		// Serialization
		//void WriteToFile( const char* path, ID3D11Device* device, ID3D11DeviceContext* context );
		//void ReadFromFile( const wchar* path, ID3D11Device* device );

		// Accessors
		std::vector<MeshMaterial>& Materials() { return meshMaterials; };
		const std::vector<MeshMaterial>& Materials() const { return meshMaterials; };

		std::vector<Mesh>& Meshes() { return meshes; }
		const std::vector<Mesh>& Meshes() const { return meshes; }

	protected:

		static void LoadMaterialResources( MeshMaterial& material, const std::string& directory, ID3D11Device* device );

		std::vector<Mesh> meshes;
		std::vector<MeshMaterial> meshMaterials;
	};

} // namespace spad

