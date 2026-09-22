// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Tree Pack/Standard/Bark/Bark Optimized"
{
	Properties
	{
		_MainTex("Albedo", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Lambert keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
		};

		uniform half WindDirection;
		uniform half WorldFrequency;
		uniform half WindSpeed;
		uniform half BendAmount;
		uniform sampler2D _MainTex;
		uniform half4 _MainTex_ST;


		float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
		{
			original -= center;
			float C = cos( angle );
			float S = sin( angle );
			float t = 1 - C;
			float m00 = t * u.x * u.x + C;
			float m01 = t * u.x * u.y - S * u.z;
			float m02 = t * u.x * u.z + S * u.y;
			float m10 = t * u.x * u.y + S * u.z;
			float m11 = t * u.y * u.y + C;
			float m12 = t * u.y * u.z - S * u.x;
			float m20 = t * u.x * u.z - S * u.y;
			float m21 = t * u.y * u.z + S * u.x;
			float m22 = t * u.z * u.z + C;
			float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
			return mul( finalMatrix, original ) + center;
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			half temp_output_14_0 = ( ( ase_vertex3Pos.y * cos( ( ( ( ase_worldPos.x + ase_worldPos.z ) * WorldFrequency ) + ( _Time.z * WindSpeed ) ) ) ) * BendAmount );
			half4 appendResult15 = (half4(temp_output_14_0 , 0.0 , temp_output_14_0 , 0.0));
			half4 break22 = mul( appendResult15, unity_ObjectToWorld );
			half4 appendResult23 = (half4(break22.x , 0 , break22.z , 0.0));
			half3 rotatedValue32 = RotateAroundAxis( float3( 0,0,0 ), appendResult23.xyz, float3( 0,0,0 ), WindDirection );
			v.vertex.xyz += rotatedValue32;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			o.Albedo = tex2D( _MainTex, uv_MainTex ).rgb;
			half temp_output_35_0 = 0.0;
			o.Specular = temp_output_35_0;
			o.Gloss = temp_output_35_0;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19200
Node;AmplifyShaderEditor.CommentaryNode;1;-1400.134,241.6194;Inherit;False;2402.447;754.1009;Bark Wind;21;32;29;23;22;21;17;16;15;14;13;12;11;10;9;8;7;6;5;4;3;2;Bark Wind;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;2;-1350.134,331.7114;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;5;-1252.421,787.4335;Inherit;False;Global;WindSpeed;Wind Speed;5;0;Create;True;0;0;0;False;0;False;1;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-1336.568,504.3422;Inherit;False;Global;WorldFrequency;World Frequency;7;0;Create;True;0;0;0;False;0;False;0.07;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;3;-1250.076,618.1243;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;4;-1134.19,355.29;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-912.4211,703.4335;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-991.2421,405.8713;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;9;-833.7711,520.6564;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CosOpNode;11;-750.9651,641.163;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;10;-744.0221,291.6194;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-562.472,485.5721;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-616.4751,658.342;Inherit;False;Global;BendAmount;Bend Amount;6;0;Create;True;0;0;0;False;0;False;0.04;0;0;0.2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-344.1151,517.2997;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;15;-166.3094,470.106;Inherit;True;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ObjectToWorldMatrixNode;16;-182.3594,744.116;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;57.95892,647.3835;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.BreakToComponentsNode;22;231.6264,651.8369;Inherit;True;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.Vector3Node;21;113.1907,413.9631;Inherit;False;Constant;_Vector0;Vector 0;4;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;18;601.0314,-544.1392;Inherit;False;378.0568;357.0746;Albedo;1;25;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;23;512.6795,555.8472;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;29;393.4381,879.7197;Inherit;False;Global;WindDirection;Wind Direction;8;0;Create;True;0;0;0;False;0;False;2.85;0;0;6.25;0;1;FLOAT;0
Node;AmplifyShaderEditor.RotateAboutAxisNode;32;682.3129,701.0878;Inherit;False;False;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;25;650.5978,-466.618;Inherit;True;Property;_MainTex;Albedo;0;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1348.798,-468.4999;Half;False;True;-1;2;ASEMaterialInspector;0;0;Lambert;Tree Pack/Standard/Bark/Bark Optimized;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.RangedFloatNode;35;1133.466,-364.1523;Inherit;False;Constant;_Float0;Float 0;1;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
WireConnection;4;0;2;1
WireConnection;4;1;2;3
WireConnection;7;0;3;3
WireConnection;7;1;5;0
WireConnection;8;0;4;0
WireConnection;8;1;6;0
WireConnection;9;0;8;0
WireConnection;9;1;7;0
WireConnection;11;0;9;0
WireConnection;12;0;10;2
WireConnection;12;1;11;0
WireConnection;14;0;12;0
WireConnection;14;1;13;0
WireConnection;15;0;14;0
WireConnection;15;2;14;0
WireConnection;17;0;15;0
WireConnection;17;1;16;0
WireConnection;22;0;17;0
WireConnection;23;0;22;0
WireConnection;23;1;21;2
WireConnection;23;2;22;2
WireConnection;32;1;29;0
WireConnection;32;3;23;0
WireConnection;0;0;25;0
WireConnection;0;3;35;0
WireConnection;0;4;35;0
WireConnection;0;11;32;0
ASEEND*/
//CHKSM=BDFB100E628290F2836388056A7F7705EAD9B215