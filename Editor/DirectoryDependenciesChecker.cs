using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kogane.Internal
{
	/// <summary>
	/// 指定したフォルダ内のアセットがフォルダ外のアセットを参照していないかどうか確認するエディタ拡張
	/// </summary>
	internal static class DirectoryDependenciesChecker
	{
		//================================================================================
		// 定数
		//================================================================================
		private const string NAME           = "UniDirectoryDependenciesChecker";
		private const string MENU_ITEM_NAME = "Assets/" + NAME + "/フォルダ内のアセットがフォルダ外のアセットを参照していないか確認";
		private const string LOG_TAG        = "[" + NAME + "]";
		
		//================================================================================
		// 関数（static）
		//================================================================================
		/// <summary>
		/// 選択中のフォルダ内のアセットがフォルダ外のアセットを参照していないかどうか確認します
		/// </summary>
		[MenuItem( MENU_ITEM_NAME )]
		private static void Check()
		{
			var canCheck = CanCheck();

			if ( !canCheck )
			{
				Debug.LogWarning( $"{LOG_TAG} 参照を確認したいフォルダを1つだけ選択した状態で実行してください" );
				return;
			}

			var obj          = Selection.activeObject;
			var path         = AssetDatabase.GetAssetPath( obj );
			var dependencies = string.Join( "\n", GetList( path ) );

			if ( dependencies.Length <= 0 )
			{
				Debug.Log( $"{LOG_TAG} 「{path}」フォルダ内のアセットはフォルダ外のアセットを参照していません" );
				return;
			}

			Debug.LogWarning( $"{LOG_TAG}「{path}」フォルダ内のアセットはフォルダ外のアセットを参照しています\n{dependencies}" );
		}

		/// <summary>
		/// 参照を確認できる場合 true を返します
		/// </summary>
		private static bool CanCheck()
		{
			if ( Selection.objects == null ) return false;
			if ( Selection.objects.Length != 1 ) return false;

			var obj         = Selection.activeObject;
			var path        = AssetDatabase.GetAssetPath( obj );
			var isDirectory = Directory.Exists( path );

			return isDirectory;
		}

		/// <summary>
		/// 指定されたフォルダ内のアセットが参照しているフォルダ外のアセットのパスをすべて返します
		/// </summary>
		public static IEnumerable<string> GetList( string path )
		{
			if ( !Directory.Exists( path ) ) yield break;

			var roots = Directory
					.GetFiles( path, "*.*", SearchOption.AllDirectories )
					.Select( x => x.Replace( "\\", "/" ) )
					.ToArray()
				;

			var dependencies = AssetDatabase
					.GetDependencies( roots )
					.Where( x => !x.StartsWith( path ) )
					.Where( x => !x.EndsWith( ".cs" ) )
					.OrderBy( c => c )
				;

			foreach ( var n in dependencies )
			{
				yield return n;
			}
		}
	}
}