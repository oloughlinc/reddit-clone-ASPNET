const postUri = 'api/posts/'
const commentUri = 'api/comments/'
const authUri = 'api/auth/login'
const refreshUri = 'api/auth/refresh'

var authToken = null;

var currPost = null;

function loggedIn() {
    var username = localStorage.getItem("username");
    if (username != null && username != "") {
        return true;
    }
    return false;
}

async function onIndexLoad() {

    if (loggedIn()) {
        var userDisplay = document.getElementById("username-display");
        userDisplay.innerHTML = "Welcome u/" + localStorage.getItem("username");
        userDisplay.style.removeProperty("display");
        document.getElementById("btn-logout").style.removeProperty("display");
        document.getElementById("btn-login").style.display = "none";
    } else {
        var userDisplay = document.getElementById("username-display");
        userDisplay.innerHTML = "";
        userDisplay.style.display = "none";
        document.getElementById("btn-logout").style.display = "none";
        document.getElementById("btn-login").style.removeProperty("display");
    }

    // silent sign in / receive new token on page reload
    await refreshAsync(localStorage.getItem("username"), "", refreshUri);
    console.log("auth token is:: " + authToken);
   
}

async function onCommentLoad() {

    var loggedIn = false;
    var username = localStorage.getItem("username");

    if (username != null && username != "") {
        loggedIn = true;
    }

    if (loggedIn) {
        var userDisplay = document.getElementById("username-display");
        userDisplay.innerHTML = "Welcome u/" + username;
        userDisplay.style.removeProperty("display");
        document.getElementById("btn-logout").style.removeProperty("display");
        document.getElementById("btn-login").style.display = "none";
        document.getElementById("new-post-form").style.removeProperty("display");
    } else {
        var userDisplay = document.getElementById("username-display");
        userDisplay.innerHTML = "";
        userDisplay.style.display = "none";
        document.getElementById("btn-logout").style.display = "none";
        document.getElementById("btn-login").style.removeProperty("display");
        document.getElementById("new-post-form").style.display = "none";
    }

    // silent sign in / receive new token on page reload
    await refreshAsync(localStorage.getItem("username"), "", refreshUri);
    //console.log("auth token is:: " + authToken);

}

function getPosts() {
    fetch(postUri)
        .then(response => response.json())
        .then(data => _displayPosts(data))
        .catch(error => console.error('Unable to get posts.', error));
}

// To be replaced with templating once one is made.
function _displayPosts(data) {
    const body = document.getElementById('body');

    data.forEach(item => {

        var posterDiv = document.createElement("div");
        posterDiv.innerHTML += "<b>u/" + item.poster + "</b>";
        posterDiv.style.fontSize = "14px";
        body.appendChild(posterDiv);

        var div = document.createElement("div");
        div.innerHTML += '<a href="./comments.html?' + item.id + '"<b>'  + item.title + "</b></a>";
        body.appendChild(div);

        var linkDiv = document.createElement("div");
        linkDiv.innerHTML += "<a href=" + item.link + ">" + item.link + "</a>";
        body.appendChild(linkDiv);

        var contentDiv = document.createElement("div");
        contentDiv.innerHTML += item.body;
        body.appendChild(contentDiv);

        var hr = document.createElement("hr");
        body.appendChild(hr);

    })
}

function getPost() {
    var thisUrl = window.location.href;
    var postId = thisUrl.split('?')[1];

    fetch(postUri + postId)
        .then(response => response.json())
        .then(data => _displayPost(data))
        .catch(error => console.error('Unable to get post.', error));
}

// To be replaced with templating once one is made.
function _displayPost(item) {
    const body = document.getElementById('post');
    
    currPost = item.id;

    var posterDiv = document.createElement("div");
    posterDiv.innerHTML += "u/";
    posterDiv.innerHTML += item.poster;
    body.appendChild(posterDiv);

    var div = document.createElement("div");
    div.innerHTML += '<b>'  + item.title + "</b>";
    div.style.fontSize = "36px";
    body.appendChild(div);

    var linkDiv = document.createElement("div");
    linkDiv.innerHTML += "<a href=" + item.link + ">" + item.link + "</a>";
    body.appendChild(linkDiv);

    var contentDiv = document.createElement("div");
    contentDiv.innerHTML += item.body;
    body.appendChild(contentDiv);

    var space = document.createElement("div");
    space.style.height = "50px";
    body.appendChild(space);

}

function getComments() {
    var thisUrl = window.location.href;
    var commentId = thisUrl.split('?')[1];

    fetch(commentUri + commentId)
        .then(response => response.json())
        .then(data => _displayComments(data))
        .catch(error => (console.error("Unable to get comments.", error)));
}

function _displayComments(data) {
    const body = document.getElementById('body');
    var depth = 0;
    _displayCommentsRecurse(data, depth);
}


// templating
function _displayCommentsRecurse(item, depth) {

    item.forEach((item) => {
        
        var template = document.querySelector("#comment");
        var comment = template.content.cloneNode(true);
    
        var thisMargin = 15 * depth;
        comment.querySelector("#comment-container").style.marginLeft = thisMargin + "px";
    
        comment.querySelector("#poster").innerHTML += "<b>u/" + item.comment.poster + "</b>";
        comment.querySelector("#body").innerHTML += item.comment.body;
        
        var container = comment.querySelector("#upsends-reply-container");
            container.querySelector("#upsends").innerHTML += "<i>Upsends: " + item.comment.upsends + "</i>";
            if (loggedIn()) container.querySelector("#reply").style.removeProperty("display");
            container.querySelector("#reply").onclick = () => { onReplyClick(item.comment.id); };


        comment.querySelectorAll("form")[0].setAttribute("id", "reply" + item.comment.id);
        comment.querySelectorAll("textarea")[0].setAttribute("id", "reply-text" + item.comment.id);
        comment.querySelector("#reply-post-btn").onclick = () => { testFunction(item.comment.id); };
        
        body.appendChild(comment);
    
        if (item.replies) {
            depth++;
            _displayCommentsRecurse(item.replies, depth);
            depth--;
        }
    });
}

// no templating
function _displayCommentsRecurse1(item, depth) {

    item.forEach(item => {

        var commentParentDiv = document.createElement("div");

        var posterDiv = document.createElement("div");
        posterDiv.innerHTML += "<b>u/" + item.comment.poster + "</b>";
        posterDiv.style.fontSize = "12px";
        commentParentDiv.appendChild(posterDiv);

        var div = document.createElement("div");
        div.innerHTML += item.comment.body;
        commentParentDiv.appendChild(div);

        var upsends = document.createElement("div");
        upsends.innerHTML += "<i>Upsends: " + item.comment.upsends + "</i>";
        commentParentDiv.appendChild(upsends);

        var replyBtn = document.createElement("button");
        replyBtn.innerHTML = "<b>Reply</b>";
        replyBtn.onclick = function() { onReplyClick(item.comment.id); };
        replyBtn.style.fontSize = "12px";
        commentParentDiv.appendChild(replyBtn);

        var hr = document.createElement("hr");
        commentParentDiv.appendChild(hr);

        var thisMargin = depth * 15;
        commentParentDiv.style.marginLeft = thisMargin + "px";

        body.appendChild(commentParentDiv);

        // visual nesting handled by incrementing a depth scalar before the next recursive call
        if (item.replies) {
            depth++;
            _displayCommentsRecurse(item.replies, depth);
            depth--;
        }
        
    });

}

function validate() {

    var valid = true;

    var userField = document.getElementById("username");
    var pwField = document.getElementById("password");

    if (userField.value == "") {
        userField.style.border = "1px solid red";
        valid = false;
    } else {
        userField.style.removeProperty("border");
    }

    if (pwField.value == "") {
        pwField.style.border = "1px solid red";
        valid = false;
    } else {
        pwField.style.removeProperty("border");
    }

    return valid;
}

async function authenticate(username, password, endpoint) {

    var response = await fetch(endpoint, {

        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            "username": username,
            "password": password
        })
    });

    if (response.status != 200) {
        return false;
    }

    var data = await response.json();
    // store token in-memory
    authToken = data.token;
    // store username in localstorage
    localStorage.setItem("username", username);

    return true;
}

async function onLoginClick() {

    if (!validate()) return;

    var username = document.getElementById("username").value;
    var password = document.getElementById("password").value;

    var authenticated = await authenticate(username, password, authUri);

    if (authenticated) {
        //location.href = "/";
        location.reload();
        return;
    }
    document.getElementById("login-invalid").style.visibility = "visible";

}

async function onLogoutClick() {

    const authLogoutUri = "api/auth/logout";
    
    var username = localStorage.getItem("username");
    var response = await fetch(authLogoutUri, {

        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            "username": username,
        })
    });

    console.log("logout response status: " + response.status);

    localStorage.removeItem("username");
    authToken = null;

    location.href = "/";
}

async function onPostCommentClick() {
    var comment = document.getElementById("new-comment").value;
    if (await postComment(0, comment)) {
        comment.value = "";
        location.href = "/comments.html?" + currPost;
    }
}

async function onReplyClick(id) {
    console.log("my id is: " + id);
    document.getElementById("reply" + id).style.removeProperty("display");
}

async function postComment(parentID, comment) {

    console.log(parentID);
    console.log(comment);
    console.log(currPost);

    await refreshAsync();

    var response = await fetch("api/comments", {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'Authorization': "Bearer " + authToken
        },
        body: JSON.stringify({
            "replyBody": comment,
            "parentID": parentID,
            "postID": currPost
        })
    });

    console.log(response.status);

    if (response.status != 201) {
        console.log("New post failed..");
        return false;
    }
    return true;

}

function showLogin() {
    const loginForm = document.getElementById("login");
    loginForm.style.visibility = "visible"
}

function hideLogin() {
    const loginForm = document.getElementById("login");
    loginForm.style.visibility = "hidden"
    document.getElementById("login-invalid").style.visibility = "hidden";
    document.getElementById("username").style.removeProperty("border");
    document.getElementById("password").style.removeProperty("border");
}

async function refreshAsync() {
    const refreshUri = "api/auth/refresh";
    var username = localStorage.getItem("username");
    if (username == null || username == "") return;
    var result = await authenticate(username, "", refreshUri);
    if (!result) {

        // refresh failed..
        // prompt user to log back in
        // the most likely cause of this is expired refresh cookie
        onLogoutClick();
    }

}

async function testFunction(id) {
    var comment = document.getElementById("reply-text" + id).value;     
    if (await postComment(id, comment)) {
        comment.value = "";
        location.href = "/comments.html?" + currPost;
    }
}