component.data = function () {
    return {
        news: [],
        slides: [],
        tokenTable: [],
        tokenTableSource: [],
        searchText: '',
        control: {
            tab: 'eos',
            fav: false
        },
        sortControl: {
            desc: 0,
            row: null
        },
        favoriteObj: {},
        onScroll: false
    };
};

component.created = function () {
    var self = this;
    app.mobile.nav = 'home';
    self.getNews();
    if (this.isSignedIn) {
        this.getFavoriteList();
    }
    qv.createView(`/api/v1/lang/${app.lang}/slides`, {}, 60000)
        .fetch(x => {
            self.slides = x.data;
        });
    qv.get(`/api/v1/lang/${app.lang}/token`, {}).then(res => {
        if (res.code - 0 === 200) {
            self.tokenTable = res.data;
            self.tokenTable.forEach(x => {
                x.current_price = x.current_price.toFixed(8);
                x.max_price_recent_day = x.max_price_recent_day.toFixed(8);
                x.min_price_recent_day = x.min_price_recent_day.toFixed(8);
                var symbol = '';
                x.change_recent_day *= 100;
                if (x.change_recent_day > 0) {
                    x.up = true;
                    x.down = false;
                    symbol = '+';
                } else if (x.change_recent_day < 0) {
                    x.up = false;
                    x.down = true;
                }
                x.change_recent_day = symbol + x.change_recent_day.toFixed(2) + '%';
            });
            self.tokenTableSource = JSON.parse(JSON.stringify(self.tokenTable));
            if (app.isMobile()) {
                this.sortTokenOnClick('change_recent_day');
            }
        }
    })
    window.addEventListener('scroll', x => {
        this.onScroll = document.documentElement.scrollTop != 0;
    })
};

component.methods = {
    doFavCallBack() {
        var self = this;
        this.delayRefresh(function () {
            self.getFavoriteList();
        });
    },
    delayRefresh(callback) {
        setInterval(callback, 3000);
    },
    getNews: function () {
        var self = this;
        qv.createView(`/api/v1/lang/${app.lang}/news`, {}, 60000)
            .fetch(x => {
                self.news = x.data;
            });
    },
    searchToken: function () {
        if (this.searchText !== '') {
            this.tokenTable = this.tokenTableSource.filter(item => {
                return item.symbol.toUpperCase().includes(this.searchText.toUpperCase())
            })
        } else {
            this.tokenTable = JSON.parse(JSON.stringify(this.tokenTableSource));
        }
    },
    formatTime(time) {
        return moment(time).format('MM-DD');
    },
    sortTokenOnClick(row) {
        if (app.isMobile()) {
            this.sortControl.row = row;
            this.sortByDecrement(row);
            return;
        }
        this.sortControl.desc = (this.sortControl.desc + 1) % 3;
        this.sortToken(row, this.sortControl.desc);
    },

    sortByDecrement(row) {
        this.tokenTable = this.tokenTable.sort((a, b) => {
            return parseFloat(b[row]) - parseFloat(a[row])
        })
    },
    sortByIncrement(row) {
        this.tokenTable = this.tokenTable.sort((a, b) => {
            return parseFloat(a[row]) - parseFloat(b[row])
        })
    },
    sortToken(row, desc) {
        this.sortControl.row = row;
        this.sortControl.desc = desc;
        if (this.sortControl.desc === 2) {
            this.sortByDecrement(row);
        } else if (this.sortControl.desc === 1) {
            this.sortByIncrement(row);
        } else {
            this.tokenTable = JSON.parse(JSON.stringify(this.tokenTableSource));
        }
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
            this.tokenTable = JSON.parse(JSON.stringify(this.tokenTableSource));
        }
    },
    backTop() {
        document.body.scrollTop = 0;
        document.documentElement.scrollTop = 0;
    }
};

component.computed = {
    isSignedIn: function () {
        return !(app.account == null || app.account.name == null);
    }
};

component.watch = {
    '$root.lang': function () {
        this.getNews();
    },
    '$root.isSignedIn': function (val) {
        if (val === true) {
            this.getFavoriteList();
        }
        //logout
        else {
            //comments: stop qv job or use signalR
        }
    },
    deep: true
};