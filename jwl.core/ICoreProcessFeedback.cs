namespace jwl.core;
using jwl.infra;

public interface ICoreProcessFeedback
    : IDisposable
{
    void PreloadAvailableWorklogTypesStart();
    void PreloadAvailableWorklogTypesEnd();
    void ReadCsvInputStart();
    void ReadCsvInputEnd();
    void RetrieveWorklogsForDeletionStart();
    void RetrieveWorklogsForDeletionSetTarget(int count);
    void RetrieveWorklogsForDeletionProcess(MultiTaskProgress progress);
    void RetrieveWorklogsForDeletionEnd();
    void OverallProcessStart();
    void OverallProcessEnd();
}
