using Rpa.Curso.Domain.Models;

namespace Rpa.Curso.Domain.Interfaces.Repositories
{
    public interface IKnowledgePlatformRepository
    {
        KnowledgePlatform Get(string name);
        void Register(KnowledgePlatform knowledgePlatform);
    }
}
