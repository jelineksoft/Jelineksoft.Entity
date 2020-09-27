namespace Jelineksoft.Entity
{
    public abstract class DatabaseBase
    {
        protected DatabaseBase(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
        
    
    }
}