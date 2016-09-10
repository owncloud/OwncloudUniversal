namespace OwncloudUniversal.Shared.Model
{
    public class AbstractItem
    { 
        public virtual long Id { get; set; }
        public virtual FolderAssociation Association { get; set; }
        public virtual string EntityId { get; set; }
        public virtual bool IsCollection { get; set; }
        public virtual string ChangeKey { get; set; }//wenn sich changekey ändert changenum erhöhen
        public virtual long ChangeNumber { get; set; }
    }
}
