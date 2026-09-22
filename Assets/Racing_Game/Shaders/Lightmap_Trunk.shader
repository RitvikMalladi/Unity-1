// Made with Amplify Shader Editor v1.9.7.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Lightmap/Trunk Lightmap"
{
	Properties
	{
		_MainTex("Albedo", 2D) = "white" {}
		_LightMap("Lightmap", 2D) = "black" {}
		_LightmapExposure1("Lightmap Exposure", Range( 0 , 2)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#define ASE_VERSION 19701
		#pragma surface surf Lambert keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float2 uv2_texcoord2;
		};

		uniform sampler2D _MainTex;
		uniform half4 _MainTex_ST;
		uniform sampler2D _LightMap;
		uniform half4 _LightMap_ST;
		uniform half _LightmapExposure1;

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			half4 tex2DNode12 = tex2D( _MainTex, uv_MainTex );
			float2 uv1_LightMap = i.uv2_texcoord2 * _LightMap_ST.xy + _LightMap_ST.zw;
			o.Albedo = ( tex2DNode12 * ( tex2D( _LightMap, uv1_LightMap ) * _LightmapExposure1 ) ).rgb;
			half temp_output_21_0 = 0.0;
			half3 temp_cast_1 = (temp_output_21_0).xxx;
			o.Emission = temp_cast_1;
			o.Specular = temp_output_21_0;
			o.Gloss = temp_output_21_0;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19701
Node;AmplifyShaderEditor.CommentaryNode;9;-1232,-320;Inherit;False;444.2924;316.8572;Albedo;1;12;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;10;-1168,32;Inherit;True;Property;_LightMap;Lightmap;1;0;Create;False;0;0;0;False;0;False;-1;None;None;True;1;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;11;-1136,272;Inherit;False;Property;_LightmapExposure1;Lightmap Exposure;2;0;Create;True;0;0;0;False;0;False;1;0.7;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;12;-1152,-224;Inherit;True;Property;_MainTex;Albedo;0;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-752,128;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-304,-32;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-144,48;Inherit;False;Constant;_Float0;Float 0;4;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;48,-64;Half;False;True;-1;2;ASEMaterialInspector;0;0;Lambert;Lightmap/Trunk Lightmap;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;True;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;13;0;10;0
WireConnection;13;1;11;0
WireConnection;14;0;12;0
WireConnection;14;1;13;0
WireConnection;0;0;14;0
WireConnection;0;2;21;0
WireConnection;0;3;21;0
WireConnection;0;4;21;0
WireConnection;0;10;12;4
ASEEND*/
//CHKSM=F0E24883CEF7D44D8311B6EC579FFF0781618CB1