using Firebase.Auth;
using Firebase.Extensions;
using Google;
using System;
using System.Threading.Tasks;
using MySelf.Model;

/// <summary>
/// Authentication
/// </summary>
public sealed partial class FirebaseManager : GameFrameworkModule, IFirebaseManager
{
    private FirebaseAuth auth;
    private FirebaseUser user;
#if !UNITY_IOS && !AmazonStore
    private Task<GoogleSignInUser> signIn;
#endif

    public FirebaseAuth Auth { get => auth; }
    
    public void InitAuth()
    {
        if (isInitAuth)return;

        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged -= AuthStateChanged;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);

        isInitAuth = true;

        Log.Info("Init Firebase Auth Success");
    }

    public void ClearAuth()
    {
        Log.Info("Clear Auth...");

        isInitAuth = false;

        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }

    /// <summary>
    /// 通过Google登录
    /// </summary>
    public void SigninWithGoogle(Action<bool> callback = null)
    {
#if !UNITY_IOS && !AmazonStore
        InitAuth();

        if (GoogleSignIn.Configuration == null)
        {
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                WebClientId = "943083155926-faq75ogqvnit1n64bmmd6g82jglntesq.apps.googleusercontent.com"
            };
        }

        GameManager.CurState = "SigninWithGoogle";
        signIn = GoogleSignIn.DefaultInstance.SignIn();
        TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();
        signIn.ContinueWithOnMainThread(task =>
        {
            Log.Info("SignIn Continue " + task.Result.IdToken);
            if (task.IsCanceled)
            {
                signInCompleted.SetCanceled();
                callback?.Invoke(false);
            }
            else if (task.IsFaulted)
            {
                signInCompleted.SetException(task.Exception);
                Log.Error("GoogleSignIn ERROR "+task.Exception);
                callback?.Invoke(false);
            }
            else
            {
                Credential credential = GoogleAuthProvider.GetCredential(((Task<GoogleSignInUser>)task).Result.IdToken, null);
                auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
                {
                    if (authTask.IsCanceled)
                    {
                        signInCompleted.SetCanceled();
                        Log.Info("Sign In Canceled");
                        callback?.Invoke(false);
                    }
                    else if (authTask.IsFaulted)
                    {
                        Log.Info("Sign In Faulted");
                        signInCompleted.SetException(authTask.Exception);
                        Log.Error("Firebase Auth Task Error "+authTask.Exception);
                        callback?.Invoke(false);
                    }
                    else
                    {
                        signInCompleted.SetResult(((Task<FirebaseUser>)authTask).Result);
                        FirebaseUser newUser = authTask.Result;
                        if(!string.IsNullOrEmpty(newUser.DisplayName))
                            GameManager.PlayerData.SetPlayerNameByLogin(newUser.DisplayName);
                        GameManager.PlayerData.UserID=auth.CurrentUser.UserId;
                        GameManager.PlayerData.LoginSdkName = LoginType.Google;
                        if (string.IsNullOrEmpty(GameManager.PlayerData.UserName))
                        {
                            GameManager.PlayerData.UserName = auth.CurrentUser.DisplayName;
                        }
                        Log.Info("User signed in successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
                        callback?.Invoke(true);
                    }
                });
            }
        });
#endif
    }

    /// <summary>
    /// 通过Facebook登录
    /// </summary>
    public void SigninWithFacebook(Action<bool> callback = null)
    {
        callback?.Invoke(false);

        //未配置id，暂时屏蔽
        //InitAuth();

        //GameManager.CurState = "SigninWithFacebook";
        //TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();
        //FacebookSignIn.DefaultInstance.Login((result) =>
        //{
        //    if (result)//链接成功
        //    {
        //        Firebase.Auth.Credential credential =
        //        Firebase.Auth.FacebookAuthProvider.GetCredential(FacebookSignIn.DefaultInstance.authCode);
        //        auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
        //        {
        //            if (authTask.IsCanceled)
        //            {
        //                Log.Info("SignInWithCredentialAsync was canceled.");
        //                callback?.Invoke(false);
        //                signInCompleted.SetCanceled();
        //            }
        //            else if (authTask.IsFaulted)
        //            {
        //                Log.Info("SignInWithCredentialAsync encountered an error: " + authTask.Exception);
        //                callback?.Invoke(false);
        //                signInCompleted.SetException(authTask.Exception);
        //            }
        //            else
        //            {
        //                Firebase.Auth.FirebaseUser newUser = authTask.Result;
        //                Log.Info("User signed in successfully: {0} ({1})",
        //                    newUser.DisplayName, newUser.UserId);
        //                if(!string.IsNullOrEmpty(newUser.DisplayName)) GameManager.PlayerData.SetPlayerNameByLogin(newUser.DisplayName);
        //                GameManager.PlayerData.UserID=auth.CurrentUser.UserId;
        //                GameManager.PlayerData.LoginSdkName = LoginType.Facebook;
        //                if (string.IsNullOrEmpty(GameManager.PlayerData.UserName))
        //                {
        //                    GameManager.PlayerData.UserName = auth.CurrentUser.DisplayName;
        //                }
        //                callback?.Invoke(true);
        //                signInCompleted.SetResult(((Task<FirebaseUser>)authTask).Result);
        //            }
        //        });
        //    }
        //    else
        //    {
        //        signInCompleted.SetCanceled();
        //        callback?.Invoke(false);
        //    }
        //});
    }

    public void SignOut()
    {
        Log.Info("sign out");

        ClearRecordLoginData();
        Auth.SignOut();
    }

    private void ClearRecordLoginData()
    {
        GameManager.PlayerData.UserID=string.Empty;
        GameManager.PlayerData.LoginSdkName =LoginType.None;
        GameManager.PlayerData.UserName =string.Empty;
    }

    // Track state changes of the auth object.
    private void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Log.Info("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Log.Info("Signed in " + user.UserId);
            }
        }
    }
}
