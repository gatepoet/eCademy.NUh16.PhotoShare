var ImageViewModel = function (data) {
    ko.mapping.fromJS(data, {}, this);

    this.thumbnailUrl = ko.computed(function () {
        return this.imageUrl() + '?thumb=200'
    }, this);
}
ImageViewModel.mapping = {
    key: function (item) {
        return ko.utils.unwrapObservable(item.id);
    },
    create: function (options) {
        return new ImageViewModel(options.data);
    },
    observe: null,
};

var HomeViewModel = function () {
    this.photos = ko.observableArray([]);
    this.loading = ko.observable(true);
    this.loadImages = function () {
        this.loading(true);
        $.get('/api/Images')
            .done(function (data) {
                ko.mapping.fromJS(data, ImageViewModel.mapping, this.photos);
                this.loading(false);
            }.bind(this))
            .fail(function (err) {
                console.error(err);
                this.loading(false);
            }.bind(this));
    }.bind(this);
};


$(function () {
    var viewModel = new HomeViewModel();
    ko.applyBindings(viewModel);
    viewModel.loadImages();
});