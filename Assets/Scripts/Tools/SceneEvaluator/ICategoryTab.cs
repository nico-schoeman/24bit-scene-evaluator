#if (UNITY_EDITOR)
namespace Tools
{
    public interface ICategoryTab
    {
        void Scan();
        void Draw();
    }
}
#endif