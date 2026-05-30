namespace Metaball
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
    internal unsafe struct InplaceList<T> where T : unmanaged
    {
        public const uint Capacity = 64;
        public const uint ValueSizeAllocation = 64;
        public static readonly uint ValueSize = (uint)sizeof(T);

        [System.Runtime.InteropServices.FieldOffset(0)]
        public ulong count;
        [System.Runtime.InteropServices.FieldOffset(16)]
        public fixed byte values[(int)(Capacity * ValueSizeAllocation)];

        public readonly ulong Count => count;
        public static InplaceList<T> Create()
        {
            if (ValueSize > ValueSizeAllocation)
            {
                throw new System.InvalidOperationException($"InplaceList value size {ValueSize} exceeds allocation size {ValueSizeAllocation}");
            } else
            {                
                return new InplaceList<T>()
                {
                    count = 0,
                };
            }
        }
        public static void Destroy(ref InplaceList<T> list)
        {
            list.count = 0;
        }

        public readonly void ReserveExact(ulong additionalCapacity)
        {
            if (Capacity - Count < additionalCapacity)
            {
                throw new System.OutOfMemoryException($"InplaceList capacity exceeded: {Count} + {additionalCapacity} > {Capacity}");
            }
        }


        public readonly ref readonly T GetUnchecked(ulong index)
        {
            fixed (byte* ptr = values)
            {
                T* itemPtr = (T*)(ptr + index * ValueSize);
                return ref *itemPtr;
            }
        }
        public readonly ref T GetMutUnchecked(ulong index)
        {
            fixed (byte* ptr = values)
            {
                T* itemPtr = (T*)(ptr + index * ValueSize);
                return ref *itemPtr;
            }
        }
        public readonly T GetCopyUnchecked(ulong index)
        {
            return GetUnchecked(index);
        }
        public readonly ref readonly T Get(ulong index)
        {
            if (index >= Count)
            {
                throw new System.IndexOutOfRangeException($"InplaceList index out of range: {index} >= {Count}");
            }
            return ref GetUnchecked(index);
        }
        public readonly ref T GetMut(ulong index)
        {
            if (index >= Count)
            {
                throw new System.IndexOutOfRangeException($"InplaceList index out of range: {index} >= {Count}");
            }
            return ref GetMutUnchecked(index);
        }
        public readonly T GetCopy(ulong index)
        {
            return Get(index);
        }

        public void Add(T item)
        {
            ReserveExact(1);
            GetMutUnchecked(Count) = item;
            ++count;
        }
        public void AddRange(System.ReadOnlySpan<T> items)
        {
            ReserveExact((ulong)items.Length);
            for (int i = 0; i < items.Length; ++i)
            {
                GetMutUnchecked(Count) = items[i];
                ++count;
            }
        }
        public void Pop()
        {
            if (Count == 0)
            {
                throw new System.InvalidOperationException($"InplaceList is empty");
            }
            else
            {
                --count;
            }
        }
        public void SwapLastPop(ulong index)
        {
            if (Count == 0)
            {
                throw new System.InvalidOperationException($"InplaceList is empty");
            }
            else if (index != Count - 1)
            {
                GetMut(index) = GetUnchecked(Count - 1);
            }
            --count;
        }

        public void Truncate(ulong newCount)
        {
            if (newCount > Count)
            {
                throw new System.InvalidOperationException($"InplaceList cannot be truncated to a larger size: {newCount} > {Count}");
            }
            else
            {
                count = newCount;
            }
        }
        public void Clear()
        {
            count = 0;
        }

        
        System.Span<T> AsSpan()
        {
            fixed (byte* ptr = values)
            {
                T* itemPtr = (T*)ptr;
                return new System.Span<T>(itemPtr, (int)Count);
            }
        }
    }
}