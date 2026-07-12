import { useState } from 'react';
import {
  Box, Typography, Button, CircularProgress, Alert, Card, Table, TableHead, TableRow,
  TableCell, TableBody, Chip, Dialog, DialogTitle, DialogContent, DialogActions, TextField, MenuItem,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { usePpeCatalogue, usePpeMutations } from '../../hooks/usePpe';

const CATEGORIES = [
  'SafetyHelmet', 'SafetyBoots', 'SafetyGlasses', 'EarPlugs', 'EarMuffs', 'Respirator', 'DustMask',
  'FaceShield', 'ReflectiveVest', 'Overalls', 'LeatherGloves', 'RubberGloves', 'FallArrestHarness',
  'WeldingHelmet', 'FireResistantClothing', 'SafetyGoggles', 'Other',
];

export default function PpeCataloguePage() {
  const { data, isLoading, error } = usePpeCatalogue();
  const { createCatalogueItem } = usePpeMutations();
  const [open, setOpen] = useState(false);
  const [form, setForm] = useState({
    itemName: '', category: 'SafetyHelmet', quantityInStock: 0, minimumStockLevel: 5, description: '',
  });

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" mb={2}>
        <Typography variant="h4">PPE Catalogue</Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setOpen(true)}>Add Item</Button>
      </Box>
      {isLoading && <CircularProgress />}
      {error && <Alert severity="error">{(error as Error).message}</Alert>}
      <Card>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Item Name</TableCell>
              <TableCell>Category</TableCell>
              <TableCell>In Stock</TableCell>
              <TableCell>Min Level</TableCell>
              <TableCell>Status</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {(data ?? []).map((item) => (
              <TableRow key={item.id} sx={item.isLowStock ? { bgcolor: 'warning.light' } : undefined}>
                <TableCell>{item.itemName}</TableCell>
                <TableCell>{item.category}</TableCell>
                <TableCell>{item.quantityInStock}</TableCell>
                <TableCell>{item.minimumStockLevel}</TableCell>
                <TableCell>
                  {item.isLowStock ? <Chip label="Low stock" color="warning" size="small" /> : <Chip label="OK" color="success" size="small" />}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Card>

      <Dialog open={open} onClose={() => setOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Add PPE Catalogue Item</DialogTitle>
        <DialogContent sx={{ display: 'grid', gap: 2, pt: 1 }}>
          <TextField label="Item Name" value={form.itemName} onChange={(e) => setForm({ ...form, itemName: e.target.value })} />
          <TextField select label="Category" value={form.category} onChange={(e) => setForm({ ...form, category: e.target.value })}>
            {CATEGORIES.map((c) => <MenuItem key={c} value={c}>{c}</MenuItem>)}
          </TextField>
          <TextField type="number" label="Quantity In Stock" value={form.quantityInStock} onChange={(e) => setForm({ ...form, quantityInStock: Number(e.target.value) })} />
          <TextField type="number" label="Minimum Stock Level" value={form.minimumStockLevel} onChange={(e) => setForm({ ...form, minimumStockLevel: Number(e.target.value) })} />
          <TextField label="Description" multiline value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={() => { createCatalogueItem.mutate(form); setOpen(false); }}>Save</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
