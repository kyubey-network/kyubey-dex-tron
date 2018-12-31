component.data = function () {
    return {
        news: [],
        page: -1,
        noMore: false,
        keyWords: '',
        boolNoData: false
    };
};

component.created = function () {
    this.init();
};

component.methods = {
    init: function () {
        this.news = [];
        this.page = -1;
        this.noMore = false;
        this.keyWords = '';
        this.loadMore();
    },
    loadMore: function () {
        var self = this;
        if (self.noMore) {
            return;
        }
        ++self.page;
        qv.get(`/api/v1/lang/${app.lang}/news`, { skip: 10 * self.page, take: 10 })
            .then(x => {
                if (!x.data.length || x.data.length < 10) {
                    self.noMore = true;
                }

                for (var i = 0; i < x.data.length; i++) {
                    var item = x.data[i];
                    item.expand = false;
                    item.content = '';
                    self.news.push(item);
                    var id = x.data[i].id;
                    qv.get(`/api/v1/lang/${app.lang}/news/${id}`)
                        .then(y => {
                            var summaryLines = y.data.content.split('\n').slice(0, 3);
                            var targetItem = self.news.filter(z => z.id === id)[0];
                            Vue.set(targetItem, 'summary', app.marked(summaryLines.join('\n')));
                            self.news.filter(z => z.id === id)[0].content = app.marked(y.data.content);
                            item.expand = true;
                        });
                }

                var time = new Date(item.time);
                item.time = moment(time + 'Z').format('YYYY-MM-DD HH:mm:ss');
            });
    }
};

component.computed = {
    newsForm: function () {
        this.boolNoData = false;
        if (this.keyWords != '') {
            var filteredNews = this.news.filter(x => {
                if (x.title.includes(this.keyWords)) {
                    return true;
                }
                else if (x.content.includes(this.keyWords)) {
                    return true;
                }
                else if (x.time.includes(this.keyWords)) {
                    return true;
                }
            });
            if (filteredNews.length == 0) {
                this.boolNoData = true;
            }
            return filteredNews;
        }
        else {
            return this.news;
        }
    }
}

component.watch = {
    //reload multi language ajax method
    '$root.lang': function () {
        this.init();
    }
}