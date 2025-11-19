namespace jwl.Core;

public interface ICoreProcessFeedback
    : IDisposable
{
    void FillJiraWithWorklogsStart();

    void FillJiraWithWorklogsSetTarget(int numberOfWorklogsToInsert, int numberOfWorklogsToDelete);

    void FillJiraWithWorklogsProcess(MultiTaskStats progress);

    void FillJiraWithWorklogsEnd();

    void NoExistingWorklogsToDelete();

    void NoFilesOnInput();

    void NoWorklogsToFill();

    void OverallProcessStart();

    void OverallProcessEnd();

    void PreloadAvailableWorklogTypesStart();

    void PreloadAvailableWorklogTypesEnd();

    void PreloadUserInfoStart(string userName);

    void PreloadUserInfoEnd();

    void ReadCsvInputStart();

    void ReadCsvInputSetTarget(int numberOfInputFiles);

    void ReadCsvInputProcess(MultiTaskStats progress);

    void ReadCsvInputEnd();

    void RetrieveWorklogsForDeletionStart();

    void RetrieveWorklogsForDeletionSetTarget(int count);

    void RetrieveWorklogsForDeletionProcess(MultiTaskStats progress);

    void RetrieveWorklogsForDeletionEnd();
}
