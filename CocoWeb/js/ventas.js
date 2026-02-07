// 1. CARGA DE DATOS DESDE LA API
async function cargarHistorialVentas() {
    const endpoint = 'https://localhost:7095/api/RepuestosVentas';
    const tabla = document.getElementById('tabla-ventas');
    
    try {
        const response = await fetch(endpoint);
        if (!response.ok) throw new Error(`Error ${response.status}`);
        const ventas = await response.json();

        let html = '';
        ventas.forEach(v => {
            const precio = v.precioVentaUnitarioEnARS || 0;
            const cantidad = v.cantidadVendida || 0;
            const ganancia = v.gananciaTotalVenta || 0; 

            const soloFecha = v.fechaVenta.split('T')[0]; 
            const partes = soloFecha.split('-'); 
            const fechaParaTabla = `${partes[2]}/${partes[1]}/${partes[0]}`;

            html += `
                <tr>
                    <td>${fechaParaTabla}</td>
                    <td>${v.repuestoStock ? v.repuestoStock.descripcion : 'Sin descripción'}</td>
                    <td class="fw-bold">$${precio.toLocaleString('es-AR')}</td>
                    <td class="text-center">${cantidad}</td>
                    <td class="text-success">+$${ganancia.toLocaleString('es-AR')}</td>
                    <td><span class="badge bg-info text-dark">${v.origenVenta}</span></td>
                </tr>
            `;
        });

        tabla.innerHTML = html;
        
        recalcularGananciaTotal();

    } catch (error) {
        console.error("Error:", error);
        tabla.innerHTML = `<tr><td colspan="6" class="text-danger text-center">Fallo la conexión: ${error.message}</td></tr>`;
    }
}

function recalcularGananciaTotal() {
    const filas = document.querySelectorAll('#tabla-ventas tr');
    let sumaTotal = 0;

    filas.forEach(fila => {

        if (fila.style.display !== 'none' && fila.cells.length >= 5) {
            
            let textoGanancia = fila.cells[4].innerText;

            let gananciaLimpia = textoGanancia
                .replace('+', '')
                .replace('$', '')
                .replace(/\./g, '') 
                .replace(',', '.')  
                .trim();

            const valorSuma = parseFloat(gananciaLimpia);
            
            if (!isNaN(valorSuma)) {
                sumaTotal += valorSuma;
            }
        }
    });

    const elementoTotal = document.getElementById('monto-total-ganancia');
    if (elementoTotal) {
        elementoTotal.innerText = `$${sumaTotal.toLocaleString('es-AR')}`;
    }
}

function filtrarPorMes() {
    const filtro = document.getElementById('filtro-mes').value;
    const filas = document.querySelectorAll('#tabla-ventas tr');

    filas.forEach(fila => {
        if (fila.cells.length < 2) return;

        const fechaTexto = fila.cells[0].innerText.trim();
        const partes = fechaTexto.split('/');
        const anioMesTabla = `${partes[2]}-${partes[1]}`;

        if (!filtro || anioMesTabla === filtro) {
            fila.style.display = ''; 
        } else {
            fila.style.display = 'none'; 
        }
    });

    recalcularGananciaTotal();
}

function limpiarFiltro() {
    document.getElementById('filtro-mes').value = '';
    filtrarPorMes();
}

document.addEventListener('DOMContentLoaded', cargarHistorialVentas);