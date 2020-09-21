using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Text;
using Android.Views;
using Android.Widget;
using Java.Lang;
using PlayTube.Activities.PlayersView;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.UserReponse;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTubeClient.Classes.Global;
using PlayTubeClient.RestCalls;
using Exception = System.Exception;

namespace PlayTube.Activities.ResponseComments
{
    public class ResponseCommentClickListener : Java.Lang.Object, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback, MaterialDialog.IInputCallback
    {
        private readonly Activity Context;
        private readonly MediaPlayerActivity MainContext;
        private CommentObject CommentObject;
        private ReplyObject ReplyObject;
        private string TypeDialog;
        private readonly string TypeClass;

        public ResponseCommentClickListener(Activity context, string typeClass)
        {
            Context = context;
            MainContext = MediaPlayerActivity.GetInstance();
            TypeClass = typeClass;
        }

        public void MoreCommentPostClick(CommentObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "MoreComment";
                    CommentObject = item;

                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                    arrayAdapter.Add(Context.GetString(Resource.String.Lbl_CopeText));

                    if (CommentObject?.IsCommentOwner != null && (bool)CommentObject?.IsCommentOwner || CommentObject?.CommentUserData?.Id == UserDetails.UserId)
                    {
                        arrayAdapter.Add(Context.GetString(Resource.String.Lbl_Edit));
                        arrayAdapter.Add(Context.GetString(Resource.String.Lbl_Delete));
                    }

                    dialogList.Title(Context.GetString(Resource.String.Lbl_More));
                    dialogList.Items(arrayAdapter);
                    dialogList.PositiveText(MainContext.GetText(Resource.String.Lbl_Close)).OnNegative(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Menu >> Delete Comment
        private void DeleteCommentEvent(CommentObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "DeleteComment";
                    CommentObject = item;

                    var dialog = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);
                    dialog.Title(MainContext.GetText(Resource.String.Lbl_DeleteComment));
                    dialog.Content(MainContext.GetText(Resource.String.Lbl_AreYouSureDeleteComment));
                    dialog.PositiveText(MainContext.GetText(Resource.String.Lbl_Yes)).OnPositive(this);
                    dialog.NegativeText(MainContext.GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.ItemsCallback(this).Build().Show();
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Event Menu >> Edit Comment
        private void EditCommentEvent(CommentObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "EditComment";
                    CommentObject = item;

                    var dialog = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                    dialog.Title(Resource.String.Lbl_Edit);
                    dialog.Input(Context.GetString(Resource.String.Lbl_Write_comment), Methods.FunString.DecodeString(item.Text), this);

                    dialog.InputType(InputTypes.TextFlagImeMultiLine);
                    dialog.PositiveText(MainContext.GetText(Resource.String.Lbl_Update)).OnPositive(this);
                    dialog.NegativeText(MainContext.GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                    dialog.Build().Show();
                    dialog.AlwaysCallSingleChoiceCallback();
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void MoreReplyPostClick(ReplyObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "MoreComment";
                    ReplyObject = item;

                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                    arrayAdapter.Add(Context.GetString(Resource.String.Lbl_CopeText));

                    if (ReplyObject?.IsReplyOwner != null && (bool)ReplyObject?.IsReplyOwner || ReplyObject?.ReplyUserData?.Id == UserDetails.UserId)
                    {
                        arrayAdapter.Add(Context.GetString(Resource.String.Lbl_Edit));
                        arrayAdapter.Add(Context.GetString(Resource.String.Lbl_Delete));
                    }

                    dialogList.Title(Context.GetString(Resource.String.Lbl_More));
                    dialogList.Items(arrayAdapter);
                    dialogList.PositiveText(MainContext.GetText(Resource.String.Lbl_Close)).OnNegative(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Menu >> Delete Reply
        private void DeleteReplyEvent(ReplyObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "DeleteComment";
                    ReplyObject = item;

                    var dialog = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);
                    dialog.Title(MainContext.GetText(Resource.String.Lbl_DeleteComment));
                    dialog.Content(MainContext.GetText(Resource.String.Lbl_AreYouSureDeleteComment));
                    dialog.PositiveText(MainContext.GetText(Resource.String.Lbl_Yes)).OnPositive(this);
                    dialog.NegativeText(MainContext.GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.ItemsCallback(this).Build().Show();
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Event Menu >> Edit Reply
        private void EditReplyEvent(ReplyObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "EditComment";
                    ReplyObject = item;

                    var dialog = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                    dialog.Title(Resource.String.Lbl_Edit);
                    dialog.Input(Context.GetString(Resource.String.Lbl_Write_comment), Methods.FunString.DecodeString(item.Text), this);

                    dialog.InputType(InputTypes.TextFlagImeMultiLine);
                    dialog.PositiveText(MainContext.GetText(Resource.String.Lbl_Update)).OnPositive(this);
                    dialog.NegativeText(MainContext.GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                    dialog.Build().Show();
                    dialog.AlwaysCallSingleChoiceCallback();
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                string text = itemString.ToString();
                if (text == Context.GetString(Resource.String.Lbl_CopeText))
                {
                    if (TypeClass == "Comment")
                        Methods.CopyToClipboard(MainContext, Methods.FunString.DecodeString(CommentObject.Text));
                    else if (TypeClass == "Reply")
                        Methods.CopyToClipboard(MainContext, Methods.FunString.DecodeString(ReplyObject.Text));
                }
                else if (text == Context.GetString(Resource.String.Lbl_Edit))
                {
                    if (TypeClass == "Comment")
                        EditCommentEvent(CommentObject);
                    else if (TypeClass == "Reply")
                        EditReplyEvent(ReplyObject);
                }
                else if (text == Context.GetString(Resource.String.Lbl_Delete))
                {
                    if (TypeClass == "Comment")
                        DeleteCommentEvent(CommentObject);
                    else if (TypeClass == "Reply")
                        DeleteReplyEvent(ReplyObject);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                    if (TypeDialog == "DeleteComment")
                    {
                        MainContext.RunOnUiThread(() =>
                        {
                            try
                            {
                                if (TypeClass == "Comment")
                                {
                                    //TypeClass
                                    var adapterGlobal = MainContext?.CommentsFragment?.MAdapter;
                                    var dataGlobal = adapterGlobal?.CommentList?.FirstOrDefault(a => a.Id == CommentObject?.Id);
                                    if (dataGlobal != null)
                                    {
                                        var index = adapterGlobal.CommentList.IndexOf(dataGlobal);
                                        if (index > -1)
                                        {
                                            adapterGlobal.CommentList.RemoveAt(index);
                                            adapterGlobal.NotifyItemRemoved(index);

                                            var y = adapterGlobal.CommentList.Count;
                                            MainContext.txtCommentNumber.Text = y.ToString(System.Globalization.CultureInfo.InvariantCulture);
                                            int index2 = MainContext.mediaList.FindIndex(x => x.RvId == MainContext.selectedMedia.RvId);
                                            MainContext.mediaList[index2].Comments = y.ToString();
                                        }
                                    }

                                    //var adapterGlobal1 = GlobalPlayerActivity.GetInstance()?.CommentsFragment?.MAdapter;
                                    //var dataGlobal1 = adapterGlobal1?.CommentList?.FirstOrDefault(a => a.Id == CommentObject?.Id);
                                    //if (dataGlobal1 != null)
                                    //{
                                    //    var index = adapterGlobal1.CommentList.IndexOf(dataGlobal1);
                                    //    if (index > -1)
                                    //    {
                                    //        adapterGlobal1.CommentList.RemoveAt(index);
                                    //        adapterGlobal1.NotifyItemRemoved(index);
                                    //    }
                                    //}

                                    //PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.Delete_Comment_Http(CommentObject.Id.ToString()) });
                                    Task.Run(async () =>
                                    {
                                        using (var client = new System.Net.Http.HttpClient())
                                        {
                                            var formContent = new System.Net.Http.FormUrlEncodedContent(new[]
                                            {
                                                new KeyValuePair<string, string>("server_key", "0913cbbd8c729a5db4db40e4aa267a17"),
                                                new KeyValuePair<string, string>("type", "delete"),
                                                new KeyValuePair<string, string>("comment_id", CommentObject.Id.ToString()),
                                                new KeyValuePair<string, string>("user_id", UserDetails.UserId),
                                                new KeyValuePair<string, string>("s", UserDetails.AccessToken)
                                            });

                                            //  send a Post request  
                                            var uri = PlayTubeClient.Client.WebsiteUrl + "/api/v1.0/?type=response_comments";
                                            var result = await client.PostAsync(uri, formContent);
                                            if (result.IsSuccessStatusCode)
                                            {
                                                // handling the answer  
                                                var resultString = await result.Content.ReadAsStringAsync();
                                                var jConfigObject = Newtonsoft.Json.Linq.JObject.Parse(resultString);
                                                if (jConfigObject["api_status"].ToString() == "200")
                                                {
                                                    //int index = MainContext.mediaList.FindIndex(x => x.RvId == MainContext.selectedMedia.RvId);
                                                    //var y = Convert.ToDouble(MainContext.mediaList[index].Comments);
                                                    //if (y > 0) y--;
                                                    //else y = 0;
                                                    //MainContext.txtCommentNumber.Text = y.ToString(System.Globalization.CultureInfo.InvariantCulture);
                                                    //MainContext.mediaList[index].Comments = y.ToString();
                                                }
                                            }
                                        }
                                    });
                                }
                                else if (TypeClass == "Reply")
                                {
                                    //TypeClass
                                    var adapterGlobal = ReplyResponseCommentBottomSheet.GetInstance()?.ReplyAdapter;
                                    var dataGlobal = adapterGlobal?.ReplyList?.FirstOrDefault(a => a.Id == ReplyObject?.Id);
                                    if (dataGlobal != null)
                                    {
                                        var index = adapterGlobal.ReplyList.IndexOf(dataGlobal);
                                        if (index > -1)
                                        {
                                            adapterGlobal.ReplyList.RemoveAt(index);
                                            adapterGlobal.NotifyItemRemoved(index);
                                        }
                                    }

                                    //PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.Delete_Reply_Http(ReplyObject.Id.ToString()) });
                                    Task.Run(async () =>
                                    {
                                        using (var client = new System.Net.Http.HttpClient())
                                        {
                                            var formContent = new System.Net.Http.FormUrlEncodedContent(new[]
                                            {
                                                new KeyValuePair<string, string>("server_key", "0913cbbd8c729a5db4db40e4aa267a17"),
                                                new KeyValuePair<string, string>("type", "delete"),
                                                new KeyValuePair<string, string>("reply_id", ReplyObject.Id.ToString()),
                                                new KeyValuePair<string, string>("user_id", UserDetails.UserId),
                                                new KeyValuePair<string, string>("s", UserDetails.AccessToken)
                                            });

                                            //  send a Post request  
                                            var uri = PlayTubeClient.Client.WebsiteUrl + "/api/v1.0/?type=response_comments";
                                            var result = await client.PostAsync(uri, formContent);
                                            if (result.IsSuccessStatusCode)
                                            {
                                                // handling the answer  
                                                var resultString = await result.Content.ReadAsStringAsync();
                                                var jConfigObject = Newtonsoft.Json.Linq.JObject.Parse(resultString);
                                                if (jConfigObject["api_status"].ToString() == "200")
                                                {
                                                    //int index = MainContext.mediaList.FindIndex(x => x.RvId == MainContext.selectedMedia.RvId);
                                                    //var y = Convert.ToDouble(MainContext.mediaList[index].Comments);
                                                    //if (y > 0) y--;
                                                    //else y = 0;
                                                    //MainContext.txtCommentNumber.Text = y.ToString(System.Globalization.CultureInfo.InvariantCulture);
                                                    //MainContext.mediaList[index].Comments = y.ToString();
                                                }
                                            }
                                        }
                                    });
                                }

                                Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CommentSuccessfullyDeleted), ToastLength.Short).Show();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        });
                    }
                    else
                    {
                        if (p1 == DialogAction.Positive)
                        {
                        }
                        else if (p1 == DialogAction.Negative)
                        {
                            p0.Dismiss();
                        }
                    }
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnInput(MaterialDialog p0, ICharSequence p1)
        {
            try
            {
                if (p1.Length() > 0)
                {
                    var strName = p1.ToString();

                    if (!Methods.CheckConnectivity())
                    {
                        Toast.MakeText(MainContext, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    }
                    else
                    {
                        if (TypeClass == "Comment")
                        {
                            //TypeClass
                            var adapterGlobal = MainContext?.CommentsFragment?.MAdapter;
                            var dataGlobal = adapterGlobal?.CommentList?.FirstOrDefault(a => a.Id == CommentObject?.Id);
                            if (dataGlobal != null)
                            {
                                dataGlobal.Text = strName;
                                var index = adapterGlobal.CommentList.IndexOf(dataGlobal);
                                if (index > -1)
                                {
                                    adapterGlobal.NotifyItemChanged(index);
                                }
                            }

                            //var adapterGlobal1 = GlobalPlayerActivity.GetInstance()?.CommentsFragment?.MAdapter;
                            //var dataGlobal1 = adapterGlobal1?.CommentList?.FirstOrDefault(a => a.Id == CommentObject?.Id);
                            //if (dataGlobal1 != null)
                            //{
                            //    dataGlobal1.Text = strName;
                            //    var index = adapterGlobal1.CommentList.IndexOf(dataGlobal1);
                            //    if (index > -1)
                            //    {
                            //        adapterGlobal1.NotifyItemChanged(index);
                            //    }
                            //}
                            //PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.Edit_Comment_Http(CommentObject.Id.ToString(), strName) });
                            Task.Run(async () =>
                            {
                                using (var client = new System.Net.Http.HttpClient())
                                {
                                    var formContent = new System.Net.Http.FormUrlEncodedContent(new[]
                                    {
                                        new KeyValuePair<string, string>("server_key", "0913cbbd8c729a5db4db40e4aa267a17"),
                                        new KeyValuePair<string, string>("type", "edit"),
                                        new KeyValuePair<string, string>("comment_id", CommentObject.Id.ToString()),
                                        new KeyValuePair<string, string>("text", strName),
                                        new KeyValuePair<string, string>("user_id", UserDetails.UserId),
                                        new KeyValuePair<string, string>("s", UserDetails.AccessToken)
                                    });

                                    //  send a Post request  
                                    var uri = PlayTubeClient.Client.WebsiteUrl + "/api/v1.0/?type=response_comments";
                                    var result = await client.PostAsync(uri, formContent);
                                    if (result.IsSuccessStatusCode)
                                    {
                                        // handling the answer  
                                        var resultString = await result.Content.ReadAsStringAsync();
                                        var jConfigObject = Newtonsoft.Json.Linq.JObject.Parse(resultString);
                                        if (jConfigObject["api_status"].ToString() == "200")
                                        {
                                        }
                                    }
                                }
                            });
                        }
                        else if (TypeClass == "Reply")
                        {
                            //TypeClass
                            var adapterGlobal = ReplyResponseCommentBottomSheet.GetInstance()?.ReplyAdapter;
                            var dataGlobal = adapterGlobal?.ReplyList?.FirstOrDefault(a => a.Id == ReplyObject?.Id);
                            if (dataGlobal != null)
                            {
                                dataGlobal.Text = strName;
                                var index = adapterGlobal.ReplyList.IndexOf(dataGlobal);
                                if (index > -1)
                                {
                                    adapterGlobal.NotifyItemChanged(index);
                                }
                            }

                            //PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.Edit_Comment_Reply_Http(ReplyObject.Id.ToString(), strName) });
                            Task.Run(async () =>
                            {
                                using (var client = new System.Net.Http.HttpClient())
                                {
                                    var formContent = new System.Net.Http.FormUrlEncodedContent(new[]
                                    {
                                        new KeyValuePair<string, string>("server_key", "0913cbbd8c729a5db4db40e4aa267a17"),
                                        new KeyValuePair<string, string>("type", "edit"),
                                        new KeyValuePair<string, string>("reply_id", ReplyObject.Id.ToString()),
                                        new KeyValuePair<string, string>("text", strName),
                                        new KeyValuePair<string, string>("user_id", UserDetails.UserId),
                                        new KeyValuePair<string, string>("s", UserDetails.AccessToken)
                                    });

                                    //  send a Post request  
                                    var uri = PlayTubeClient.Client.WebsiteUrl + "/api/v1.0/?type=response_comments";
                                    var result = await client.PostAsync(uri, formContent);
                                    if (result.IsSuccessStatusCode)
                                    {
                                        // handling the answer  
                                        var resultString = await result.Content.ReadAsStringAsync();
                                        var jConfigObject = Newtonsoft.Json.Linq.JObject.Parse(resultString);
                                        if (jConfigObject["api_status"].ToString() == "200")
                                        {
                                        }
                                    }
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion MaterialDialog 
    }
}