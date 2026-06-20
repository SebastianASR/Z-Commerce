// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function abrirCarritoLateral(idProducto = null) {
    let url = "/Home/GetCarritoOffcanvas";
    if (idProducto !== null) {
        url += "?idProducto=" + idProducto;
    }

    // Llamamos al servidor en segundo plano
    fetch(url)
        .then(response => response.text())
        .then(htmlRecibido => {
            // 1. Metemos el HTML que cocinó C# dentro del contenedor del cajón
            document.getElementById("contenedor-carrito-offcanvas").innerHTML = htmlRecibido;

            // 2. Le decimos a Bootstrap: "Despliega el cajón lateral"
            let cajonElemento = document.getElementById("offcanvasCarrito");
            let cajonOffcanvas = bootstrap.Offcanvas.getOrCreateInstance(cajonElemento);
            cajonOffcanvas.show();
        })
        .catch(error => console.error("Error al cargar el carrito:", error));
}