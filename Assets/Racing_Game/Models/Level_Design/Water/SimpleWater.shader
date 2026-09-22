// Made with Amplify Shader Editor v1.9.7.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "LightingBox/Water/Simple Water"
{
	Properties
	{
		_Color("Color", Color) = (0.7843137,0.8901961,0.7333333,1)
		_Smoothness("Smoothness", Range( 0 , 1)) = 1
		_Metallic("Metallic", Range( 0 , 1)) = 0.554
		_WaterSpeed("Speed", Range( 0 , 2)) = 0
		[Normal]_NormalMap_1("NormalMap 1", 2D) = "bump" {}
		_NormalScale("Normal Scale", Range( 0 , 1)) = 0.1
		[Normal]_NormalMap_2("NormalMap 2", 2D) = "bump" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#pragma target 4.6
		#define ASE_VERSION 19701
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _NormalMap_1;
		uniform float _WaterSpeed;
		uniform float4 _NormalMap_1_ST;
		uniform float _NormalScale;
		uniform sampler2D _NormalMap_2;
		uniform float4 _Color;
		uniform float _Metallic;
		uniform float _Smoothness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_NormalMap_1 = i.uv_texcoord * _NormalMap_1_ST.xy + _NormalMap_1_ST.zw;
			float2 panner7 = ( ( _Time.x * _WaterSpeed ) * float2( 1,1 ) + uv_NormalMap_1);
			float2 panner25 = ( ( ( _Time.x * -1.0 ) * _WaterSpeed ) * float2( 1,1 ) + uv_NormalMap_1);
			float3 lerpResult28 = lerp( UnpackScaleNormal( tex2D( _NormalMap_1, panner7 ), _NormalScale ) , UnpackScaleNormal( tex2D( _NormalMap_2, panner25 ), _NormalScale ) , 0.5);
			o.Normal = lerpResult28;
			o.Albedo = _Color.rgb;
			o.Metallic = _Metallic;
			float temp_output_5_0 = _Smoothness;
			o.Smoothness = temp_output_5_0;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19701
Node;AmplifyShaderEditor.CommentaryNode;41;-2244,-178;Inherit;False;1780;1107;Normal;16;23;33;34;32;8;24;35;9;36;7;25;40;31;39;4;28;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TimeNode;23;-1618,640;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;33;-1570,816;Inherit;False;Constant;_Float2;Float 2;6;0;Create;True;0;0;0;False;0;False;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-2194,208;Inherit;False;Property;_WaterSpeed;Speed;3;0;Create;False;0;0;0;False;0;False;0;0.5;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;8;-1634,48;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-1362,672;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;9;-1602,-128;Inherit;False;0;4;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-1378,160;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;24;-1618,480;Inherit;False;0;4;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-1250,496;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;7;-1218,0;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;1,1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-2194,336;Inherit;False;Property;_NormalScale;Normal Scale;5;0;Create;False;0;0;0;False;0;False;0.1;0.049;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;25;-1090,688;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;1,1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;4;-1010,80;Inherit;True;Property;_NormalMap_1;NormalMap 1;4;1;[Normal];Create;False;0;0;0;False;0;False;-1;None;eb67116e85eb1ed478d630ced7325072;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;0.1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;31;-882,384;Inherit;False;Constant;_Float1;Float 1;6;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;39;-818,544;Inherit;True;Property;_NormalMap_2;NormalMap 2;6;1;[Normal];Create;False;0;0;0;False;0;False;-1;None;eb67116e85eb1ed478d630ced7325072;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;0.1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;10;301.7835,699.4836;Float;False;Property;_Metallic;Metallic;2;0;Create;True;0;0;0;False;0;False;0.554;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-27.83771,655.7706;Float;False;Property;_Smoothness;Smoothness;1;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;3;-388,-65;Float;False;Property;_Color;Color;0;0;Create;True;0;0;0;False;0;False;0.7843137,0.8901961,0.7333333,1;0.4352083,0.5660378,0.4654272,0.591;False;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;421.0958,360.0903;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;28;-642,320;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1252.512,188.3273;Float;False;True;-1;6;ASEMaterialInspector;0;0;Standard;LightingBox/Water/Simple Water;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;0;31.88;0;20000;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;32;0;23;1
WireConnection;32;1;33;0
WireConnection;36;0;8;1
WireConnection;36;1;34;0
WireConnection;35;0;32;0
WireConnection;35;1;34;0
WireConnection;7;0;9;0
WireConnection;7;1;36;0
WireConnection;25;0;24;0
WireConnection;25;1;35;0
WireConnection;4;1;7;0
WireConnection;4;5;40;0
WireConnection;39;1;25;0
WireConnection;39;5;40;0
WireConnection;17;1;5;0
WireConnection;28;0;4;0
WireConnection;28;1;39;0
WireConnection;28;2;31;0
WireConnection;0;0;3;0
WireConnection;0;1;28;0
WireConnection;0;3;10;0
WireConnection;0;4;5;0
ASEEND*/
//CHKSM=B3E295D6267553E805D62E51D4E40BF245B93819