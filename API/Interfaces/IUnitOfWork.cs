namespace API.Interfaces;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    IMessageRepository MessageRepository { get; }
    ILikesRepository LikesRepository { get; }
    IPhotosRepository PhotosRepository { get; }
    Task<bool> Complete();
    bool HasChanges();
}
