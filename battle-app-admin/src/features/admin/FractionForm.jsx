import { useParams, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { createFraction } from '../../services/api';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Box, Heading, VStack, Input, Button, createToaster, Toaster
} from '@chakra-ui/react';
import { Field } from '@chakra-ui/react';

const schema = z.object({
  fractionName: z.string().min(1, 'Fraction name is required')
});

const toaster = createToaster({
  placement: 'top-end',
  duration: 3000,
});

export default function FractionForm() {
  const { battleId } = useParams();
  const { register, handleSubmit } = useForm({
    resolver: zodResolver(schema),
    defaultValues: { fractionName: '' }
  });
  const qc = useQueryClient();
  const nav = useNavigate();

  const mutation = useMutation({
    mutationFn: (payload) => createFraction(battleId, payload),
    onSuccess: () => {
      qc.invalidateQueries(['battle', battleId]);
      toaster.create({ title: 'Fraction created', type: 'success' });
      nav(`/admin/${battleId}`);
    }
  });

  return (
    <>
      <Toaster toaster={toaster} />
      <Box>
        <Heading size="md" mb="4">Add Fraction</Heading>
        <VStack as="form" align="stretch" spacing="4" onSubmit={handleSubmit((v) => mutation.mutate(v))}>
          <Field.Root>
            <Field.Label>Fraction Name</Field.Label>
            <Input {...register('fractionName')} placeholder="Enter fraction name" />
          </Field.Root>

          <Button type="submit" colorScheme="green" isLoading={mutation.isPending}>
            Create Fraction
          </Button>
        </VStack>
      </Box>
    </>
  );
}
