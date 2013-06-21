/*
** $Id: lmem.c,v 1.70.1.1 2007/12/27 13:02:25 roberto Exp $
** Interface to Memory Manager
** See Copyright Notice in lua.h
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace KopiLua
{
	public partial class Lua
	{
		public const string MEMERRMSG	= "not enough memory";

		public static T[] LuaMReallocV<T>(LuaState L, T[] block, int new_size)
		{
			return (T[])LuaMRealloc(L, block, new_size);
		}
			
		//#define luaM_freemem(L, b, s)	luaM_realloc_(L, (b), (s), 0)
		//#define luaM_free(L, b)		luaM_realloc_(L, (b), sizeof(*(b)), 0)
		//public static void luaM_freearray(lua_State L, object b, int n, Type t) { luaM_reallocv(L, b, n, 0, Marshal.SizeOf(b)); }

		// C# has it's own gc, so nothing to do here...in theory...
		public static void LuaMFreeMem<T>(LuaState L, T b) { LuaMRealloc<T>(L, new T[] {b}, 0); }
		public static void LuaMFree<T>(LuaState L, T b) { LuaMRealloc<T>(L, new T[] {b}, 0); }
		public static void LuaMFreeArray<T>(LuaState L, T[] b) { LuaMReallocV(L, b, 0); }

		public static T LuaMMalloc<T>(LuaState L) { return (T)LuaMRealloc<T>(L); }
		public static T LuaMNew<T>(LuaState L) { return (T)LuaMRealloc<T>(L); }
		public static T[] LuaMNewVector<T>(LuaState L, int n)
		{
			return LuaMReallocV<T>(L, null, n);
		}

		public static void LuaMGrowVector<T>(LuaState L, ref T[] v, int nelems, ref int size, int limit, CharPtr e)
		{
			if (nelems + 1 > size)
				v = (T[])LuaMGrowAux(L, ref v, ref size, limit, e);
		}

		public static T[] LuaMReallocVector<T>(LuaState L, ref T[] v, int oldn, int n)
		{
			Debug.Assert((v == null && oldn == 0) || (v.Length == oldn));
			v = LuaMReallocV<T>(L, v, n);
			return v;
		}


		/*
		** About the realloc function:
		** void * frealloc (void *ud, void *ptr, uint osize, uint nsize);
		** (`osize' is the old size, `nsize' is the new size)
		**
		** Lua ensures that (ptr == null) iff (osize == 0).
		**
		** * frealloc(ud, null, 0, x) creates a new block of size `x'
		**
		** * frealloc(ud, p, x, 0) frees the block `p'
		** (in this specific case, frealloc must return null).
		** particularly, frealloc(ud, null, 0, 0) does nothing
		** (which is equivalent to free(null) in ANSI C)
		**
		** frealloc returns null if it cannot create or reallocate the area
		** (any reallocation to an equal or smaller size cannot fail!)
		*/



		public const int MINSIZEARRAY	= 4;


		public static T[] LuaMGrowAux<T>(LuaState L, ref T[] block, ref int size,
							 int limit, CharPtr errormsg)
		{
			T[] newblock;
			int newsize;
			if (size >= limit / 2)
			{  /* cannot double it? */
				if (size >= limit)  /* cannot grow even a little? */
					LuaGRunError(L, errormsg);
				newsize = limit;  /* still have at least one free place */
			}
			else
			{
				newsize = size * 2;
				if (newsize < MINSIZEARRAY)
					newsize = MINSIZEARRAY;  /* minimum size */
			}
			newblock = LuaMReallocV<T>(L, block, newsize);
			size = newsize;  /* update only when everything else is OK */
			return newblock;
		}


		public static object LuaMTooBig (LuaState L) {
		  LuaGRunError(L, "memory allocation error: block too big");
		  return null;  /* to avoid warnings */
		}



		/*
		** generic allocation routine.
		*/

		public static object LuaMRealloc(LuaState L, Type t)
		{
			int unmanaged_size = (int)GetUnmanagedSize(t);
			int nsize = unmanaged_size;
			object new_obj = System.Activator.CreateInstance(t);
			AddTotalBytes(L, nsize);
			return new_obj;
		}

		public static object LuaMRealloc<T>(LuaState L)
		{
			int unmanaged_size = (int)GetUnmanagedSize(typeof(T));
			int nsize = unmanaged_size;
			T new_obj = (T)System.Activator.CreateInstance(typeof(T));
			AddTotalBytes(L, nsize);
			return new_obj;
		}

		public static object LuaMRealloc<T>(LuaState L, T obj)
		{
			int unmanaged_size = (int)GetUnmanagedSize(typeof(T));
			int old_size = (obj == null) ? 0 : unmanaged_size;
			int osize = old_size * unmanaged_size;
			int nsize = unmanaged_size;
			T new_obj = (T)System.Activator.CreateInstance(typeof(T));
			SubtractTotalBytes(L, osize);
			AddTotalBytes(L, nsize);
			return new_obj;
		}

		public static object LuaMRealloc<T>(LuaState L, T[] old_block, int new_size)
		{
			int unmanaged_size = (int)GetUnmanagedSize(typeof(T));
			int old_size = (old_block == null) ? 0 : old_block.Length;
			int osize = old_size * unmanaged_size;
			int nsize = new_size * unmanaged_size;
			T[] new_block = new T[new_size];
			for (int i = 0; i < Math.Min(old_size, new_size); i++)
				new_block[i] = old_block[i];
			for (int i = old_size; i < new_size; i++)
				new_block[i] = (T)System.Activator.CreateInstance(typeof(T));
			if (CanIndex(typeof(T)))
				for (int i = 0; i < new_size; i++)
				{
					ArrayElement elem = new_block[i] as ArrayElement;
					Debug.Assert(elem != null, String.Format("Need to derive type {0} from ArrayElement", typeof(T).ToString()));
					elem.SetIndex(i);
					elem.SetArray(new_block);
				}
			SubtractTotalBytes(L, osize);
			AddTotalBytes(L, nsize);
			return new_block;
		}

		public static bool CanIndex(Type t)
		{
			if (t == typeof(char))
				return false;
			if (t == typeof(byte))
				return false;
			if (t == typeof(int))
				return false;
			if (t == typeof(uint))
				return false;
			if (t == typeof(LocVar))
				return false;
			return true;
		}

		static void AddTotalBytes(LuaState L, int num_bytes) { G(L).totalbytes += (uint)num_bytes; }
		static void SubtractTotalBytes(LuaState L, int num_bytes) { G(L).totalbytes -= (uint)num_bytes; }

		static void AddTotalBytes(LuaState L, uint num_bytes) {G(L).totalbytes += num_bytes;}
		static void SubtractTotalBytes(LuaState L, uint num_bytes) {G(L).totalbytes -= num_bytes;}
	}
}
