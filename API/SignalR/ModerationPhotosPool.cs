

using API.Entities;

namespace API.SignalR;

public class ModerationPhotosPool
{
    private readonly List<PhotoDto> _items = new List<PhotoDto>(0);
    private SemaphoreSlim _poolSemaphore = new SemaphoreSlim(1, 1);

    public async Task<IOperationAccessor> GetOperationAccessorAsync()
    {
        await _poolSemaphore.WaitAsync();
        return new OperationAccessor(this);
    }

    private class OperationAccessor : IOperationAccessor, IDisposable
    {
        private readonly ModerationPhotosPool _pool;
        private bool _disposed;

        public OperationAccessor(ModerationPhotosPool pool)
        {
            _pool = pool;
        }

        private List<PhotoDto> Items => _pool._items;

        public int ItemsCount => Items.Count;

        public int Capacity 
        {
            get => Items.Capacity; 
            set => Items.Capacity = value; 
        }

        public PhotoDto this[int index] 
        { 
            get
            {
                ThrowIfIndexIsOutOfRange(index);
                return Items[index];
            }
            set
            {
                ThrowIfIndexIsOutOfRange(index);
                Items[index] = value;
            }
        }

        private void ThrowIfIndexIsOutOfRange(int index)
        {
            if (index < 0 || index >= Items.Count)
            {
                throw new IndexOutOfRangeException($"The index is out of range. Index: {index}, items count: {Items.Count}");
            }
        }   

        public IEnumerable<PhotoDto> GetPhotos(int maxCount)
        {
            ThrowIfDisposed();

            if (Items == null) return Array.Empty<PhotoDto>();

            return maxCount >= _pool._items.Count
                ? _pool._items.ToArray()
                : _pool._items.GetRange(0, maxCount);
        }

        public void Dispose()
        {
            ThrowIfDisposed();
            _disposed = true;
            _pool._poolSemaphore.Release();
        }

        public void AddPhotos(IEnumerable<PhotoDto> items)
        {
            Items.AddRange(items);
        }

        public void AddPhoto(PhotoDto item)
        {
            Items.Add(item);
        }

        public int FindIndex(int photoId)
        {
            var items = Items;
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].Id == photoId) return i;
            }

            return -1;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Items.Count) throw new ArgumentException($"The index is out of range. Index: {index}, items count: {Items.Count}");
            Items.RemoveAt(index);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(OperationAccessor));
            }
        }
    }

    public interface IOperationAccessor : IDisposable
    {
        IEnumerable<PhotoDto> GetPhotos(int maxCount);
        int ItemsCount { get; }
        void AddPhotos(IEnumerable<PhotoDto> items);
        int Capacity { get; set; }
        void AddPhoto(PhotoDto item);
        int FindIndex(int photoId);
        PhotoDto this[int index] { get; set; }
        void RemoveAt(int index);
    }
}
