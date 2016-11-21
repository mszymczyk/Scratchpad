// HiStreamTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "../../../Core/Util/HiStream.h"

using namespace spad;
using namespace stream;

typedef OutputStream<true> OutStream;

namespace GraphNodeTags
{
enum Type
{
	invalid,

	dependencyNode,

	count,
};
}

namespace GraphAttributeTags
{
enum Type
{
	invalid,

	setAttribute,
	setAttributeObsolete,
	setAttributeNew,

	count,
};
}

void testWrite( OutStream& os )
{
	os.begin();



	os.pushChild( GraphNodeTags::dependencyNode );

	os.addStringFloat( GraphAttributeTags::setAttribute, "attr0", 0.0f );
	os.addStringFloat( GraphAttributeTags::setAttributeObsolete, "attr1", 1.0f );
	os.addStringFloat( GraphAttributeTags::setAttributeNew, "attr2", 2.0f );

	os.popChild();



	os.pushChild( GraphNodeTags::dependencyNode );

	os.addStringFloat( GraphAttributeTags::setAttribute, "attr3", 3.0f );
	os.addStringFloat( GraphAttributeTags::setAttributeNew, "attr4", 4.0f );
	os.addStringFloat( GraphAttributeTags::setAttributeObsolete, "attr5", 5.0f );

	os.popChild();



	os.end();
}

void testRead( const InputStream& is )
{
	Node root = is.getRoot();
	//StreamNode::const_iterator itBegin = root.begin();
	//StreamNode::const_iterator itEnd = root.end();

	size_t childNo = 0;
	for ( auto&& n : root.children() )
	{
		std::cout << "Child no: " << childNo << std::endl;
		std::cout << "Num children: " << n.numChildren() << std::endl;
		std::cout << "Num attributes: " << n.numAttributes() << std::endl;

		switch ( n.tag() )
		{
		case GraphNodeTags::dependencyNode:

			for ( auto&& a : n.attributes() )
			{
				//std::cout << "\t" << "Attr: " << 

				switch ( a.tag() )
				{
				case GraphAttributeTags::setAttribute:

					if ( a.type() == AttributeType::stringFloat )
					{
						const char* str;
						size_t strLen;
						float f;
						a.getStringFloat( str, strLen, f );

						std::cout << "\t" << "Attr: " << str << ": " << f << std::endl;
					}
					break;

				default:
					std::cout << "\t" << "Unsupported attr: " << (u32)a.tag() << std::endl;
					// skip all other attributes
					break;
				}

			}

			break;
		default:
			break;
		}

		++ childNo;
	}
}

int main()
{
	OutStream os;
	testWrite( os );

	InputStream is( os.getBuffer(), os.getBufferSize() );
	testRead( is );

	return 0;
}

