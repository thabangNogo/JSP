import { Box, Typography, Card, CardContent } from '@mui/material';
import ConstructionIcon from '@mui/icons-material/Construction';

interface PlaceholderPageProps {
  title: string;
  description?: string;
}

export default function PlaceholderPage({ title, description }: PlaceholderPageProps) {
  return (
    <Box>
      <Typography variant="h4" gutterBottom>{title}</Typography>
      <Card>
        <CardContent sx={{ textAlign: 'center', py: 8 }}>
          <ConstructionIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
          <Typography color="text.secondary">
            {description ?? `${title} module is coming soon.`}
          </Typography>
        </CardContent>
      </Card>
    </Box>
  );
}
