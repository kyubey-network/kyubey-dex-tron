component.data = function () {
    return { 
        tokens: [],
        listWithFiter :[],
        searchText: '',
        favoriteObj: {}
    };
};

component.methods = {
    searchToken: function () {
        if (this.searchText !== '') {
            this.listWithFiter = this.tokens.filter(item => {
                return item.symbol.toUpperCase().includes(this.searchText.toUpperCase())
            })
        } else {
            this.listWithFiter = this.tokens;
        }
    },

    goToEosExchange: function (symbol) {
        if (symbol === 'EOS') {
            return;
        }
        app.redirect('/exchange/:id', '/exchange/' + symbol, { id: symbol }, {})
    },

    toggleFav(token) {
        const isAdd = !this.favoriteObj[token];
        app.toggleFav(token, isAdd, () => {
            this.favoriteObj[token] = isAdd;
        })
    },

    getFavoriteList() {
        qv.get(`/api/v1/lang/${app.lang}/user/${app.account.name}/favorite`, {}).then(res => {
            if (res.code === 200) {
                let favoriteObj = {};
                let favoriteList = res.data || [];
                favoriteList.forEach((item) => {
                    favoriteObj[item.symbol] = item.favorite
                })
                this.favoriteObj = favoriteObj;
            }
        })
    },

    filterFav() {
        this.control.fav = !this.control.fav;
        if (this.control.fav) {
            this.tokenTable = this.tokenTable.filter((item) => {
                return this.favoriteObj[item.symbol]
            })
        } else {
            this.tokenTable = this.tokenTableSource
        }
    }
} 

component.computed = {
    account: function () {
        return app.account;
    },

    totalEvaluated: function () {
        if (!this.tokens.length) {
            return 0;
        }
        var values = this.tokens.map(x => x.eos);
        return values.reduce(function (prev, curr) {
            return prev + curr;
        });
    }
};

component.created = function () {
    app.mobile.nav = 'assets';
    var self = this
    if (!app.isSignedIn) {
        app.redirect('/');
    }
    this.getFavoriteList();
    qv.get(`/api/v1/lang/${app.lang}/User/${app.account.name}/wallet`, {}).then(res => {
        if (res.code - 0 === 200) {
            self.tokens = res.data;
            self.listWithFiter = res.data;
        }
    })
};