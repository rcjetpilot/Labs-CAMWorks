' Import main DriveWorks Extensibility types
Imports DriveWorks.Extensibility

' Other useful extensibility imports in DriveWorks.Applications.dll (this has been added as a reference for you)
Imports DriveWorks.Applications
Imports DriveWorks.Applications.Administrator.Extensibility
Imports DriveWorks.Applications.Autopilot.Extensibility

' Other useful extensibility imports (you will need to add a reference to use these)
Imports DriveWorks.SolidWorks                                 ' In DriveWorks.SolidWorks.dll
Imports DriveWorks.Applications.Extensibility
Imports DriveWorks.SolidWorks.Generation


<ApplicationPlugin("DriveWorks.CAMWorks", "DriveWorks CAMWorks Plugin", "DriveWorks CAMWorks")> _
Public Class CAMWorks
    Implements IApplicationPlugin

    Implements IHasConfiguration

    Private mApplication As IApplication
    Private mSettings As PluginSettings
    Private WithEvents mGenerationService As IGenerationService
    Private WithEvents mSolidWorksService As ISolidWorksService

    Private mGroupService As IGroupService

    Private mGroupSettings As Generation.GenerationSettings

    Private Const EVENT_SOURCE_NAME As String = "urn://DriveWorks/Labs/CAMWorks"

    Private mEventLog As IApplicationEventService

    Private mModelList As New List(Of String)

    Public Sub Initialize(application As DriveWorks.Applications.IApplication) Implements DriveWorks.Applications.Extensibility.IApplicationPlugin.Initialize

        mApplication = application
        mSettings = New PluginSettings(application.SettingsManager)
        ' this plugin requires 2 of the DriveWorks services
        mSolidWorksService = application.ServiceManager.GetService(Of ISolidWorksService)()
        mGenerationService = application.ServiceManager.GetService(Of IGenerationService)()

        ' Get the logging service
        mEventLog = application.ServiceManager.GetService(Of IApplicationEventService)()
        ' Load settings
        LoadSettings()

    End Sub

    Public Sub ShowConfiguration(owner As System.Windows.Forms.IWin32Window) Implements DriveWorks.Applications.Extensibility.IHasConfiguration.ShowConfiguration

        Dim configForm As New ConfigureCAMWorks(mModelList)

        If configForm.ShowDialog() = Windows.Forms.DialogResult.OK Then

            mModelList = configForm.Models

            SaveSettings()

        End If

    End Sub

#Region " Settings "

    Private Sub LoadSettings()

        Dim loadString As String = mSettings.GetSetting("ModelList", "")

        If loadString.Trim.Equals(String.Empty) Then Exit Sub

        Dim modelAdday As String() = loadString.Split("|")

        For Each model In modelAdday
            mModelList.Add(model)
        Next

    End Sub

    Private Sub SaveSettings()

        Dim saveString As String = String.Empty

        For Each Model In mModelList
            If saveString = String.Empty Then
                saveString = Model
            Else
                saveString = saveString & "|" & Model
            End If
        Next

        mSettings.SaveSetting("ModelList", saveString)
    End Sub

#End Region

    Private Sub mGenerationService_BatchStarted(sender As Object, e As EventArgs) Handles mGenerationService.BatchStarted

        ' at the point of starting batch, we MUST have a group open, so this is a good time to get the group service
        ' We need the group service so that we can ultimatelly find the Group Content Folder to know where the macros folder is.
        mGroupService = mApplication.ServiceManager.GetService(Of IGroupService)()

        Dim settingsManager As ISettingsManager = mApplication.SettingsManager

        mGroupSettings = New GenerationSettings

        mGroupSettings.Load(mGroupService.ActiveGroup, settingsManager)

    End Sub


    Private Sub mGenerationService_ModelGenerationContextCreated(sender As Object, e As DriveWorks.SolidWorks.Generation.ModelGenerationContextEventArgs) Handles mGenerationService.ModelGenerationContextCreated

        Try

            For Each modelActionName As String In mModelList

                Dim modelName As String = System.IO.Path.GetFileNameWithoutExtension(e.Context.Model.MasterPath)

                ' Let the user know that we are loaded, and a model is being generated
                AddEvent(ApplicationEventType.Information, String.Format("Testing for model name (Mapping = {0}, Model = {1})", modelActionName, modelName), True, modelActionName)

                If modelName.Equals(modelActionName, StringComparison.OrdinalIgnoreCase) Then

                    ' Let the user know that we are loaded, and a model is being generated
                    AddEvent(ApplicationEventType.Information, String.Format("Model Match found (Mapping Name = {0}) - CAM Data will be created after generation is complete.", modelActionName), True, modelActionName)

                    Dim modelGenerationContext As New CAMModelContext(e.Context, mSolidWorksService, mEventLog, EVENT_SOURCE_NAME, mGroupSettings)
                    Exit Sub
                End If
            Next

        Catch ex As Exception

            ' Let the user know that there was a major problem
            AddEvent(ApplicationEventType.Error, "Error creating CAMWorks Data.", True, ex.ToString)

        End Try

    End Sub

    Private Sub AddEvent(ByVal type As DriveWorks.Applications.ApplicationEventType, ByVal description As String, ByVal loggingEnabled As Boolean, Optional ByVal targetName As String = Nothing)
        If mEventLog Is Nothing Then
            Return
        End If

        If Not loggingEnabled Then
            Return
        End If

        mEventLog.AddEvent(type, EVENT_SOURCE_NAME, "Export CAMWorks Data", description, Nothing, targetName, Nothing)
    End Sub
End Class


