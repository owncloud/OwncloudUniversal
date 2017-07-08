namespace OwncloudUniversal.Synchronization.Processing
{
    //when adding new values to this, make sure you also add the corresponding translation
    public enum ExecutionStatus
    {
        Active = 1,
        Finished = 2,
        Stopped = 3,
        Scanning = 4,
        Deletions = 5,
        UpdatingIndex =6,
        Ready = 7,
        Sending = 8,
        Receiving = 9,
        Error = 10
    }
}
