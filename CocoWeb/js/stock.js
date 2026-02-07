// Usamos una variable global para el puerto así lo cambiás en un solo lugar
const API_URL = 'https://localhost:7095/api'; 

async function cargarStock() {
    try {
        const response = await fetch(`${API_URL}/RepuestosStock`);
        if (!response.ok) throw new Error("Error en la respuesta de la API");
        
        const datos = await response.json();
        let html = '';
        
        datos.forEach(item => {
            html += `
                <tr>
                    <td><strong>${item.descripcion}</strong></td>
                    <td><strong>${item.codigoOriginal || '-'}</strong></td>
                    <td><span class="badge bg-secondary">${item.marca}</span></td>
                    <td><strong>${item.pesoKg || 0} kg</strong></td>
                    <td>${item.cantidadEnStock}</td>
                    <td class="text-primary fw-bold">U$D ${item.costoUSD}</td>
                    <td>
                        <button class="btn btn-sm btn-success" onclick="abrirModalVenta(${item.id}, '${item.descripcion.replace(/'/g, "\\'")}', ${item.costoUSD})">
                            🛒 Vender
                        </button>
                    </td>
                    <td>
                        <button class="btn btn-sm btn-info" onclick="abrirModalStock(${item.id}, '${item.descripcion.replace(/'/g, "\\'")}', ${item.cantidadEnStock})">
                            🔧 Ajustar
                        </button>
                    </td>
                    <td>
                        <button class="btn btn-sm btn-danger" onclick="abrirModalEliminar(${item.id}, '${item.descripcion.replace(/'/g, "\\'")}')">
                            🗑️ Eliminar
                        </button>
                    </td>
                </tr>
            `;
        });
        document.getElementById('tabla-stock').innerHTML = html;
    } catch (error) {
        console.error("Error al cargar stock:", error);
    }
}

function abrirModalVenta(id, desc, costo) {
    document.getElementById('v_repuestoId').value = id;
    document.getElementById('v_costoUSD').innerText = costo;
    document.getElementById('tituloVenta').innerText = "Vender: " + desc;

    // Seteamos la fecha de hoy automáticamente al abrir
    const hoy = new Date().toISOString().split('T')[0];
    document.getElementById('v_fecha').value = hoy;
    
    let modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('modalVenta'));
    modal.show();
}

function abrirModalStock(id, desc, cant) {
    const elId = document.getElementById('s_repuestoId');
    const elDesc = document.getElementById('s_nombre_display');
    const elCant = document.getElementById('s_cantidad_input');

    // Asignamos valores solo si los elementos existen para evitar errores
    if (elId) elId.value = id;
    if (elDesc) elDesc.innerText = desc;
    if (elCant) elCant.value = cant;

    const modalElement = document.getElementById('modalStock');
    const myModal = bootstrap.Modal.getOrCreateInstance(modalElement);
    
    myModal.show();
}

function abrirModalEliminar(id, desc, cant) {
    const elId = document.getElementById('e_repuestoId');
    const elDesc = document.getElementById('e_nombre_display');

    if (elId) elId.value = id;
    if (elDesc) elDesc.innerText = desc;

    const modalElement = document.getElementById('modalEliminar');
    const myModal = bootstrap.Modal.getOrCreateInstance(modalElement);
    
    myModal.show();
}

function abrirModalNuevoProducto() {
    // Limpieza manual segura
    ['n_descripcion', 'n_marca', 'n_codigo', 'n_cantidad', 'n_costo', 'n_peso'].forEach(id => {
        const el = document.getElementById(id);
        if(el) el.value = (id === 'n_cantidad') ? 0 : "";
    });

    let modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('modalNuevoProducto'));
    modal.show();
}

async function guardarNuevoProducto() {
    const nuevo = {
        descripcion: document.getElementById('n_descripcion').value,
        marca: document.getElementById('n_marca').value,
        codigoOriginal: document.getElementById('n_codigo').value,
        cantidadEnStock: parseInt(document.getElementById('n_cantidad').value) || 0,
        costoUSD: parseFloat(document.getElementById('n_costo').value) || 0,
        pesoKg: parseFloat(document.getElementById('n_peso').value) || 0
    };

    if(!nuevo.descripcion) return alert("La descripción es obligatoria");

    try {
        const response = await fetch(`${API_URL}/RepuestosStock`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(nuevo)
        });

        if (response.ok) {
            alert("✅ Producto agregado.");
            location.reload();
        }
    } catch (error) {
        alert("Fallo la conexión con el servidor.");
    }
}

async function confirmarEliminacion() {
    const id = document.getElementById('e_repuestoId').value;
    
    try {
        const response = await fetch(`${API_URL}/RepuestosStock/${id}`, { 
            method: 'DELETE' 
        });

        if (response.ok) {
            const modalElement = document.getElementById('modalEliminar');
            const instance = bootstrap.Modal.getInstance(modalElement);
            if (instance) instance.hide();

            alert("🗑️ Producto eliminado con éxito.");
            cargarStock();
        } else {
            alert("El servidor no permitió eliminar el producto.");
        }
    } catch (error) {
        console.error("Error al eliminar:", error);
        alert("Error de conexión con la API.");
    }
}

async function guardarCambioStock() {
    const id = document.getElementById('s_repuestoId').value;
    const nuevaCantidad = parseInt(document.getElementById('s_cantidad_input').value);

    if (isNaN(nuevaCantidad)) return alert("Ingresá un número válido");

    try {
        const getRes = await fetch(`${API_URL}/RepuestosStock/${id}`);
        if (!getRes.ok) throw new Error("No se pudo obtener el producto");
        
        const producto = await getRes.json();
        producto.cantidadEnStock = nuevaCantidad;

        const putRes = await fetch(`${API_URL}/RepuestosStock/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(producto)
        });

        if (putRes.ok) {
            const modalElement = document.getElementById('modalStock');
            bootstrap.Modal.getInstance(modalElement).hide();
            alert("✅ Stock actualizado en CocoRepuestos.");
            cargarStock(); 
        } else {
            alert("El servidor rechazó la actualización (Error 400/500).");
        }
    } catch (error) {
        console.error(error);
        alert("Fallo la comunicación con la API.");
    }
}

function filtrarStock() {
    let filtro = document.getElementById('buscador').value.toLowerCase();
    let filas = document.querySelectorAll('#tabla-stock tr');
    filas.forEach(fila => {
        let texto = fila.innerText.toLowerCase();
        fila.style.display = texto.includes(filtro) ? '' : 'none';
    });
}

async function confirmarVenta() {
    const id = document.getElementById('v_repuestoId').value;
    const precioFinal = parseFloat(document.getElementById('v_precio').value);
    const origen = document.getElementById('v_origen').value;
    const fechaElegida = document.getElementById('v_fecha').value;

    if (!precioFinal) return alert("Por favor, ingresá el precio cobrado.");
    if (!fechaElegida) return alert("La fecha es obligatoria.");


    const venta = {
        repuestoId: parseInt(id),
        precioARS: precioFinal,
        origen: origen,
        fecha: new Date(fechaElegida).toISOString()
    };

    try {
        const response = await fetch(`${API_URL}/Ventas`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(venta)
        });

        if (response.ok) {
            alert("✅ Venta registrada y stock descontado.");
            const modalElement = document.getElementById('modalVenta');
            bootstrap.Modal.getInstance(modalElement).hide();
            cargarStock();
        } else {
            alert("Error al registrar: verifique si hay stock suficiente.");
        }
    } catch (error) {
        console.error("Error:", error);
        alert("Error de conexión con la API.");
    }
}

window.onload = cargarStock;