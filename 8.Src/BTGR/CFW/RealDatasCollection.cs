namespace CFW
{
    public class RealDatasCollection: Infragistics.Shared.SubObjectsCollectionBase 
    {
        const int DEFAULT_CAPACITY = 100;

        protected override int InitialCapacity
        {
            get
            {
                return DEFAULT_CAPACITY;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }
    }
}
