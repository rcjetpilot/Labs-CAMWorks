Imports DriveWorks.Applications

Friend Class PluginSettings

    Private Const SETTINGS_BASE As String = "Common\Plugins\CAMWorks\"

    Private mSettings As ISettingsManager

#Region " .ctor "

    Friend Sub New(ByVal settingsManager As ISettingsManager)
        Debug.Assert(settingsManager IsNot Nothing, "The settings manager must be specified.")
        mSettings = settingsManager
    End Sub

#End Region

#Region " Porting Helpers "

    Friend Function GetSetting(ByVal settingName As String, ByVal defaultValue As String) As String
        Dim value As String = mSettings.GetSettingAsString(SettingLevel.User, SETTINGS_BASE & settingName, False)

        If String.IsNullOrEmpty(value) Then
            Return defaultValue
        Else
            Return value
        End If
    End Function

    Friend Sub SaveSetting(ByVal settingName As String, ByVal newValue As String)
        mSettings.SetSetting(SettingLevel.User, SETTINGS_BASE & settingName, newValue, False)
    End Sub

#End Region

End Class
