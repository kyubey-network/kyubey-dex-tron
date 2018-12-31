component.data = function () {
    return {
    };
};

component.created = function () {
    app.mobile.nav = 'more';
};

component.mounted = function () { 
}

component.methods = {
};

component.computed = {
    isSignedIn: function () {
        return !(app.account == null || app.account.name == null);
    }
};

component.watch = {
    '$root.isSignedIn': function (val) {
        if (val === true) {
        } else {}
    },
    deep: true
}
