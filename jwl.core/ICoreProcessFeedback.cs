namespace jwl.core;
using jwl.infra;

public interface ICoreProcessFeedback
    : IDisposable
{
    void DeleteExistingWorklogsStart();
    void DeleteExistingWorklogsSetTarget(int numberOfWorklogs);
    void DeleteExistingWorklogsProcess(MultiTaskProgress progress);
    void DeleteExistingWorklogsEnd();
    void FillJiraWithWorklogsStart();
    void FillJiraWithWorklogsSetTarget(int numberOfWorklogs);
    void FillJiraWithWorklogsProcess(MultiTaskProgress progress);
    void FillJiraWithWorklogsEnd();
    void OverallProcessStart();
    void OverallProcessEnd();
    void PreloadAvailableWorklogTypesStart();
    void PreloadAvailableWorklogTypesEnd();
    void PreloadUserInfoStart(string userName);
    void PreloadUserInfoEnd();
    void ReadCsvInputStart();
    void ReadCsvInputSetTarget(int numberOfInputFiles);
    void ReadCsvInputProcess(MultiTaskProgress progress);
    void ReadCsvInputEnd();
    void RetrieveWorklogsForDeletionStart();
    void RetrieveWorklogsForDeletionSetTarget(int count);
    void RetrieveWorklogsForDeletionProcess(MultiTaskProgress progress);
    void RetrieveWorklogsForDeletionEnd();
}
