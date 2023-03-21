namespace jwl.core;
using jwl.infra;

public interface ICoreProcessFeedback
    : IDisposable
{
    void DeleteExistingWorklogsStart();
    void DeleteExistingWorklogsSetTarget(int numberOfWorklogs);
    void DeleteExistingWorklogsProcess(MultiTaskProgress progress);
    void DeleteExistingWorklogsEnd();
    void OverallProcessStart();
    void OverallProcessEnd();
    void PreloadAvailableWorklogTypesStart();
    void PreloadAvailableWorklogTypesEnd();
    void ReadCsvInputStart();
    void ReadCsvInputEnd();
    void RetrieveWorklogsForDeletionStart();
    void RetrieveWorklogsForDeletionSetTarget(int count);
    void RetrieveWorklogsForDeletionProcess(MultiTaskProgress progress);
    void RetrieveWorklogsForDeletionEnd();
}
