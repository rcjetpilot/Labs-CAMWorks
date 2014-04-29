Imports System.Windows.Forms

Public Class ConfigureCAMWorks

    Public Sub New(ModelList As List(Of String))

        ' This call is required by the designer.
        InitializeComponent()

        For Each Model In ModelList

            LstModels.Items.Add(Model)

        Next

        ' Add any initialization after the InitializeComponent() call.
        'Disable the Add and Delete buttons
        BtnAdd.Enabled = False
        BtnDelete.Enabled = False

    End Sub


    Private Sub BtnAdd_Click(sender As System.Object, e As System.EventArgs) Handles BtnAdd.Click

        'Get Text from Model Name
        Dim ModelEntered As String
        ModelEntered = TxtModelName.Text.Trim

        ' quick check to ensure we don't add the same model twice
        For Each item In LstModels.Items
            If item = ModelEntered Then
                MessageBox.Show("Model name has already been entered")
                Exit Sub
            End If
        Next

        'Add Text to List
        LstModels.Items.Add(ModelEntered)

        'Clear Text Box
        TxtModelName.Clear()

    End Sub

    Private Sub BtnDelete_Click(sender As System.Object, e As System.EventArgs) Handles BtnDelete.Click

        'Delete Selected Item from Model List
        Dim SelectedModel As String
        SelectedModel = LstModels.SelectedItem
        LstModels.Items.Remove(SelectedModel)
        LstModels.ClearSelected()

    End Sub

    Private Sub TxtModelName_TextChanged(sender As System.Object, e As System.EventArgs) Handles TxtModelName.TextChanged

        'Enable or disable the Add button
        If TxtModelName.Text = String.Empty Then
            BtnAdd.Enabled = False
        Else
            BtnAdd.Enabled = True

        End If

    End Sub

    Private Sub LstModels_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles LstModels.SelectedIndexChanged

        'Enable or disable the Delete button
        If LstModels.SelectedItem Is Nothing Then
            BtnDelete.Enabled = False
        Else
            BtnDelete.Enabled = True
        End If

    End Sub

    Public ReadOnly Property Models As List(Of String)

        Get
            Dim MyModels As New List(Of String)

            For Each Model In LstModels.Items

                MyModels.Add(Model)

            Next

            Return MyModels

        End Get

    End Property

End Class