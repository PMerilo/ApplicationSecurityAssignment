///<reference path="../lib/jquery/dist/jquery.js" />
$('#passwd').keyup(() => {
    var pw = $('#passwd').val()
    if (pw.search(/^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*?[#?!@$%^&*-])(.{12,})$/) == 0) {
        $('#pwvalidation').text('Password is strong')
        $('#pwvalidation').removeClass()
        $('#pwvalidation').addClass('text-success')
    } else {
        $('#pwvalidation').text('Password is too weak')
        $('#pwvalidation').removeClass()
        $('#pwvalidation').addClass('text-danger')
    }
})
