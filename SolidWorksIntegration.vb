Imports DriveWorks.Applications
Imports DriveWorks.SolidWorks
Imports System.IO
Imports SldWorks
Imports DriveWorks.Reporting
Imports SwConst

Public Class SolidWorksInteraction

    Private mSwApp As SldWorks.SldWorks
    Private mSwModel As SldWorks.ModelDoc2
    Private mSwCustpropManager As CustomPropertyManager

    Private mSolidWorksService As ISolidWorksService
    Private mGroupSettings As Generation.GenerationSettings
    Private mEventLog As IApplicationEventService

    Private Const DEFAULT_MACRO_METHOD_NAME As String = "Main"

    Public Event LogFileEvent(logText As String)

    Public Sub New(SolidWorksService As ISolidWorksService, groupSettings As Generation.GenerationSettings, ByVal eventLog As IApplicationEventService)

        mGroupSettings = groupSettings
        mEventLog = eventLog

        If SolidWorksService.SolidWorks Is Nothing Then

            mSwApp = CType(CreateObject("SldWorks.Application"), SldWorks.SldWorks)

        Else
            mSwApp = SolidWorksService.SolidWorks
        End If

        mSwApp.Visible = True

        'Attach to the current doc
        mSwModel = mSwApp.ActiveDoc

        ' get the custom property manager
        If mSwModel IsNot Nothing Then
            mSwCustpropManager = mSwModel.Extension.CustomPropertyManager("")
        End If

    End Sub

    Public Sub Dispose()
        mSwModel = Nothing
        mSwApp = Nothing
    End Sub

#Region "    SolidWorks Traversal                "

    Public Function GetModelPath() As String
        If Not mSwModel Is Nothing Then
            Return mSwModel.GetPathName
        Else
            Return ""
        End If
    End Function

    Public ReadOnly Property FilePath As String
        Get
            If Not mSwModel Is Nothing Then
                Return IO.Path.GetDirectoryName(mSwModel.GetPathName)
            Else
                Return ""
            End If
        End Get
    End Property

    Public ReadOnly Property FileName As String
        Get
            If Not mSwModel Is Nothing Then
                Return IO.Path.GetFileName(mSwModel.GetPathName)
            Else
                Return ""
            End If
        End Get
    End Property

    Public ReadOnly Property IsPart As Boolean
        Get
            Return mSwModel.GetType() = 1
        End Get
    End Property

    Public ReadOnly Property FileNameWithoutExtension As String
        Get
            If Not mSwModel Is Nothing Then
                Return IO.Path.GetFileNameWithoutExtension(mSwModel.GetPathName)
            Else
                Return ""
            End If
        End Get
    End Property

    Private Function GetCustomProperty(propertyName As String) As String

        If mSwCustpropManager Is Nothing Then Return String.Empty

        Return mSwCustpropManager.Get(propertyName)

    End Function

    Public ReadOnly Property CreateCAMData() As Boolean
        Get

            'the default is to run the cam data, that way if the custom property doesn't exist, it will run by default
            ' the only value that will stop the property from returning true is FALSE
            Dim createCAMDataValue As String = GetCustomProperty("DWCAMWorks")

            If String.IsNullOrEmpty(createCAMDataValue) Then

                Return True
            Else

                Select Case createCAMDataValue.ToLower
                    Case "false"
                        Return False
                    Case Else
                        Return True
                End Select
            End If

        End Get
    End Property

    Public ReadOnly Property PerformExtractMachineFeatures() As Boolean
        Get
            'the default is to run the cam data, that way if the custom property doesn't exist, it will run by default
            ' the only value that will stop the property from returning true is FALSE
            Dim createCAMDataValue As String = GetCustomProperty("DWCAMWorksEMF")

            If String.IsNullOrEmpty(createCAMDataValue) Then

                Return True
            Else

                Select Case createCAMDataValue.ToLower
                    Case "false"
                        Return False
                    Case Else
                        Return True
                End Select
            End If

        End Get
    End Property

    Public ReadOnly Property GenerateOperationPlanOption() As Integer
        Get
            ' GOP preference are
            ' 1 : RETAIN
            ' 2 : REGENERATE
            ' 3 : CANCEL
            ' 4 : QUERY PREFERENCES

            'We default to 2

            'the default is to run the cam data, that way if the custom property doesn't exist, it will run by default
            Dim CAMDataValue As String = GetCustomProperty("DWCAMWorksGOP")

            If String.IsNullOrEmpty(CAMDataValue) Then

                ' 2 : REGENERATE
                Return 2
            Else

                Select Case CAMDataValue.ToLower
                    Case "1"
                        Return 1
                    Case "2"
                        Return 2
                    Case "3"
                        Return 3
                    Case "4"
                        Return 4
                    Case Else
                        Return 2
                End Select
            End If


        End Get
    End Property

    Public ReadOnly Property PerformGenerateToolPath() As Boolean
        Get
            'the default is to run the cam data, that way if the custom property doesn't exist, it will run by default
            ' the only value that will stop the property from returning true is FALSE
            Dim createCAMDataValue As String = GetCustomProperty("DWCAMWorksGTP")

            If String.IsNullOrEmpty(createCAMDataValue) Then

                Return True
            Else

                Select Case createCAMDataValue.ToLower
                    Case "false"
                        Return False
                    Case Else
                        Return True
                End Select
            End If

        End Get
    End Property

    Public ReadOnly Property PostProcessPath() As String
        Get

            Dim CAMDataValue As String

            ' Get the custom property called DWCAMWorksPostFilePath
            CAMDataValue = GetCustomProperty("DWCAMWorksPostFilePath")

            ' if we don't find the custom property - return an empty string
            If String.IsNullOrEmpty(CAMDataValue) Then
                Return String.Empty
            Else

                Return CAMDataValue
            End If

        End Get
    End Property

#End Region

#Region " Macros "

    ''' <summary>
    ''' Runs a specific macro for a model.
    ''' </summary>
    ''' <param name="fullFilePath">The full path to the macro to run.</param>
    ''' <param name="macroName">The name of the method in the DriveWorks module in the macro to run (defaults to Main).</param>
    ''' <remarks></remarks>
    Friend Sub RunModelMacro(ByVal fullFilePath As String, Optional ByVal macroName As String = DEFAULT_MACRO_METHOD_NAME)
        RunMacroCore(fullFilePath, macroName)
    End Sub

    ''' <summary>
    ''' Runs a specific macro for a model.
    ''' </summary>
    ''' <param name="fileName">The name and extension of the macro without any path information.</param>
    ''' <param name="macroName">The name of the method in the DriveWorks module in the macro to run (defaults to Main).</param>
    ''' <remarks></remarks>
    Friend Sub RunGroupContentMacro(ByVal fileName As String, Optional ByVal macroName As String = DEFAULT_MACRO_METHOD_NAME)

        ' Make sure the group content folder exists before doing anything else
        Static Dim mGcfExists As Boolean = Directory.Exists(mGroupSettings.GroupContentFolder)

        If Not mGcfExists Then
            Return
        End If

        ' Run the macro
        Dim fullPath = IO.Path.Combine(mGroupSettings.GroupContentFolder, "Macros", fileName)
        RunMacroCore(fullPath, macroName)
    End Sub

    ''' <summary>
    ''' Runs a specific macro for a model.
    ''' </summary>
    ''' <param name="fileName">The name and extension of the macro without any path information.</param>
    ''' <param name="macroName">The name of the method in the DriveWorks module in the macro to run (defaults to Main).</param>
    ''' <remarks></remarks>
    Friend Sub RunSharedContentMacro(ByVal fileName As String, Optional ByVal macroName As String = DEFAULT_MACRO_METHOD_NAME)

        ' Make sure the shared content folder exists before doing anything else
        Static Dim mScfExists As Boolean = Directory.Exists(mGroupSettings.SharedContentFolder)

        If Not mScfExists Then
            Return
        End If

        ' Run the macro
        Dim fullPath = IO.Path.Combine(mGroupSettings.SharedContentFolder, "Macros", fileName)
        RunMacroCore(fullPath, macroName)
    End Sub

    Private Sub RunMacroCore(ByVal fullFilePath As String, ByVal macroName As String)
        If Not File.Exists(fullFilePath) Then
            Return
        End If

        RunMacro(mEventLog, fullFilePath, macroName)
    End Sub

    ''' <summary>
    ''' Runs a macro in a module called DriveWorks in the given macro file.
    ''' </summary>
    ''' <param name="report">The report to which to log problems/success (optional).</param>
    ''' <param name="macroFilePath">The path to the macro file.</param>
    ''' <param name="macroName">The name of the macro to run.</param>
    ''' <returns>True if the macro was successfully executed, otherwise false.</returns>
    ''' <remarks></remarks>
    Private Function RunMacro(ByVal report As IReportWriter, ByVal macroFilePath As String, ByVal macroName As String) As Boolean

        Dim result As Integer ' ByRef
        Dim success = mSwApp.RunMacro2(macroFilePath, "DriveWorks", macroName, swRunMacroOption_e.swRunMacroDefault, result)
        Dim resultNative = DirectCast(result, swRunMacroError_e)

        ' If the method doesn't exist, don't report anything
        If Not success AndAlso resultNative = swRunMacroError_e.swRunMacroError_InvalidProcname Then
            Return False
        End If

        If success Then

            'Report that the macro ran successfully
            report.WriteEntry(ReportingLevel.Normal,
                                ReportEntryType.Information,
                                "Running SolidWorks Macro",
                                String.Empty,
                                String.Format("The SolidWorks Macro '{0}' successfully ran", macroFilePath),
                                Nothing)
        Else
            'Report that the macro failed to run
            report.WriteEntry(ReportingLevel.Normal,
                                ReportEntryType.Error,
                                "Running SolidWorks Macro",
                                String.Empty,
                                String.Format("The SolidWorks Macro '{0}' failed to run", macroFilePath, resultNative.ToString),
                                Nothing)
        End If

        Return success
    End Function

#End Region

End Class
