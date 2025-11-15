import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { createBattle } from '../../services/api';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Box, Heading, VStack, Input, Button, HStack, createToaster, Toaster
} from '@chakra-ui/react';
import { Field } from '@chakra-ui/react';
import { useNavigate } from 'react-router-dom';
import { emptyBattlePayload } from '../../types/dto';

const schema = z.object({
  name: z.string().min(1),
  width: z.coerce.number().min(10).max(2000),
  height: z.coerce.number().min(10).max(2000)
});

const toaster = createToaster({
  placement: 'top-end',
  duration: 3000,
});

export default function BattleForm() {
  const { register, handleSubmit } = useForm({
    resolver: zodResolver(schema),
    defaultValues: emptyBattlePayload()
  });
  const qc = useQueryClient();
  const nav = useNavigate();

  const mutation = useMutation({
    mutationFn: (payload) => createBattle(payload),
    onSuccess: (battleId) => {
      qc.invalidateQueries(['battles']);
      toaster.create({ title: 'Battle created', type: 'success' });
      nav(`/pustka-admin-panel/${battleId}`);
    }
  });

  return (
    <>
      <Toaster toaster={toaster} />
      <Box>
        <Heading size="md" mb="4">New Battle</Heading>
        <VStack as="form" align="stretch" spacing="4" onSubmit={handleSubmit((v) => mutation.mutate(v))}>
          <Field.Root>
            <Field.Label>Name</Field.Label>
            <Input {...register('name')} />
          </Field.Root>

          <HStack>
            <Field.Root>
              <Field.Label>Width</Field.Label>
              <Input type="number" {...register('width')} />
            </Field.Root>
            <Field.Root>
              <Field.Label>Height</Field.Label>
              <Input type="number" {...register('height')} />
            </Field.Root>
          </HStack>

          <Button type="submit" colorScheme="green" isLoading={mutation.isPending}>Create</Button>
        </VStack>
      </Box>
    </>
  );
}
