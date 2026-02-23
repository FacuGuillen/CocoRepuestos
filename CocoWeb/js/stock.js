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

    const hoy = new Date().toISOString().split('T')[0];
    document.getElementById('v_fecha').value = hoy;

    // ESTA LÍNEA ES LA QUE CARGA A DANI Y A LOS DEMÁS
    cargarClientesEnSelect(); 
    
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
    
    // Usamos parseFloat para permitir decimales en el precio
    const nuevoCostoUSD = parseFloat(document.getElementById('s_costoUSD').value);

    // Validación simple
    if (isNaN(nuevaCantidad) || isNaN(nuevoCostoUSD)) {
        return alert("⚠️ Por favor, ingresá valores numéricos válidos en ambos campos.");
    }

    try {
        // Buscamos el repuesto actual
        const getRes = await fetch(`${API_URL}/RepuestosStock/${id}`);
        if (!getRes.ok) throw new Error("No se pudo obtener el producto");
        
        const producto = await getRes.json();
        
        // Actualizamos los valores
        producto.cantidadEnStock = nuevaCantidad;
        producto.costoUSD = nuevoCostoUSD;

        // Mandamos el objeto actualizado a la API
        const putRes = await fetch(`${API_URL}/RepuestosStock/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(producto)
        });

        if (putRes.ok) {
            // Cerramos el modal
            const modalElement = document.getElementById('modalStock');
            const modalInstance = bootstrap.Modal.getInstance(modalElement);
            modalInstance.hide();
            
            alert("✅ Stock y Costo actualizados correctamente.");
            cargarStock(); // Refrescamos la lista de la pantalla
        } else {
            alert("❌ Hubo un error al guardar en la base de datos.");
        }
    } catch (error) {
        console.error(error);
        alert("🚨 Error de conexión con el servidor.");
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
    // 1. CAPTURA DE DATOS (IDs del nuevo modal)
    const id = document.getElementById('v_repuestoId').value;
    const precioTotal = parseFloat(document.getElementById('v_precio').value);
    const fechaElegida = document.getElementById('v_fecha').value;
    const cantidad = parseInt(document.getElementById('v_cantidad').value) || 1;
    const tasa = parseFloat(document.getElementById('v_tasa')?.value) || 1; 
    const idCliente = document.getElementById('v_cliente').value; // El select dinámico
    const provinciaElegida = document.getElementById('v_provincia').value;

    // 2. VALIDACIONES
    if (!precioTotal) return alert("Por favor, ingresá el precio cobrado.");
    if (!fechaElegida) return alert("La fecha es obligatoria.");
    if (!idCliente) return alert("Por favor, seleccioná un cliente.");
    if (!provinciaElegida) return alert("Por favor, seleccioná una provincia.");

    // 3. OBJETO VENTA (Limpio de errores de referencia)
    const venta = {
    fechaVenta: new Date(fechaElegida).toISOString(), 
    tasaCambio: tasa,
    // Mandamos el precio unitario tal cual lo escribiste
    precioVentaUnitarioEnARS: precioTotal, 
    repuestoStockId: parseInt(id), 
    clienteId: parseInt(idCliente), 
    cantidadVendida: cantidad,
    provincia: provinciaElegida
};

    try {
        const response = await fetch(`${API_URL}/RepuestosVentas`, { 
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(venta)
        });

        if (response.ok) {
            alert("✅ Venta registrada con éxito en CocoRepuestos.");
            const modalElement = document.getElementById('modalVenta');
            bootstrap.Modal.getInstance(modalElement).hide();
            cargarStock(); // Refrescamos la tabla para ver el nuevo stock
        } else {
            const errorDetalle = await response.text();
            alert("Error al registrar: " + errorDetalle);
        }
    } catch (error) {
        console.error("Error:", error);
        alert("Error de conexión con la API.");
    }
}

async function cargarClientesEnSelect() {
    const select = document.getElementById('v_cliente');
    try {
        // Asegurate que el puerto sea 7095 y la ruta sea /api/Clientes
        const response = await fetch('https://localhost:7095/api/Clientes');
        
        if (!response.ok) throw new Error("Fallo en la respuesta");
        
        const clientes = await response.json();
        console.log("Clientes cargados:", clientes); // Esto te ayuda a ver si llega Dani

        select.innerHTML = '<option value="">-- Seleccione un Cliente --</option>';
        clientes.forEach(c => {
            const option = document.createElement('option');
            option.value = c.id; 
            option.text = c.nombre; 
            select.appendChild(option);
        });
    } catch (error) {
        console.error("Error al cargar clientes:", error);
        select.innerHTML = '<option value="">Error al cargar clientes</option>';
    }
}

window.onload = cargarStock;