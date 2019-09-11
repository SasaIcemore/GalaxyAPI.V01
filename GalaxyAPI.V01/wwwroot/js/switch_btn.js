function switch_btn_click() {
    $(".slide-btn div").on("click", function () {
        if (this.className == "inner-on") {
            this.style.left = -51 + "px";
            this.childNodes[1].checked = false;
            this.className = "inner-off";
        } else {
            this.style.left = 0;
            this.childNodes[1].checked = true;
            this.className = "inner-on";
        }
    });
}