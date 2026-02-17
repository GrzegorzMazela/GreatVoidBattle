import { useParams, useNavigate, Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { createShip, getShip } from '../../services/api';
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query';
import {
  Box,
  Heading,
  VStack,
  Field,
  Input,
  Button,
  HStack,
  Text,
  Spinner,
  Breadcrumb,
  SimpleGrid,
} from '@chakra-ui/react';
import { WeaponDefaults, getShipTypeNamePl } from '../../types/dto';
import { useState } from 'react';
import { useNotification } from '../../contexts/NotificationContext';

const cloneCountSchema = z.object({
  cloneCount: z.coerce.number().int().min(1).max(50),
});

const clonesSchema = z.object({
  clones: z.array(
    z.object({
      name: z.string().min(1, 'Nazwa jest wymagana'),
      positionX: z.coerce.number().min(0),
      positionY: z.coerce.number().min(0),
    })
  ),
});

export default function ShipCloneForm() {
  const { showSuccess, showError } = useNotification();
  const { battleId, fractionId, shipId } = useParams();
  const navigate = useNavigate();
  const qc = useQueryClient();
  const [step, setStep] = useState(1);
  const [cloneCount, setCloneCount] = useState(0);

  const { data: templateShip, isLoading: loadingShip } = useQuery({
    queryKey: ['ship', battleId, fractionId, shipId],
    queryFn: () => getShip(battleId, fractionId, shipId),
  });

  const countForm = useForm({
    resolver: zodResolver(cloneCountSchema),
    defaultValues: { cloneCount: 1 },
  });

  const clonesForm = useForm({
    resolver: zodResolver(clonesSchema),
    defaultValues: { clones: [] },
  });

  const onCountSubmit = countForm.handleSubmit((data) => {
    const n = data.cloneCount;
    const defaults = Array.from({ length: n }, (_, i) => ({
      name: `${templateShip?.name || 'Statek'} (kopia ${i + 1})`,
      positionX: (templateShip?.x ?? 0) + (i + 1) * 10,
      positionY: templateShip?.y ?? 0,
    }));
    clonesForm.reset({ clones: defaults });
    setCloneCount(n);
    setStep(2);
  });

  const createMutation = useMutation({
    mutationFn: async (payload) => createShip(battleId, fractionId, payload),
    onSuccess: () => {},
    onError: (err) => {
      showError(err.message || 'Błąd tworzenia statku');
    },
  });

  const onClonesSubmit = clonesForm.handleSubmit(async (data) => {
    if (!templateShip) return;
    const basePayload = {
      name: templateShip.name,
      type: templateShip.type,
      positionX: templateShip.x,
      positionY: templateShip.y,
      speed: templateShip.speed,
      hitPoints: templateShip.hitPoints,
      shields: templateShip.shields,
      armor: templateShip.armor,
      laserMaxRange: templateShip.laserMaxRange ?? WeaponDefaults.laserMaxRange,
      laserDamage: templateShip.laserDamage ?? WeaponDefaults.laserDamage,
      missileMaxRange: templateShip.missileMaxRange ?? WeaponDefaults.missileMaxRange,
      missileEffectiveRange: templateShip.missileEffectiveRange ?? WeaponDefaults.missileEffectiveRange,
      missileDamage: templateShip.missileDamage ?? WeaponDefaults.missileDamage,
      missileSpeed: templateShip.missileSpeed ?? WeaponDefaults.missileSpeed,
      modules: templateShip.modules?.map((m) => ({
        weaponTypes: m.weaponTypes || ['Missile', 'Laser', 'PointDefense'],
      })) ?? [],
    };

    let created = 0;
    for (const clone of data.clones) {
      try {
        await createMutation.mutateAsync({
          ...basePayload,
          name: clone.name,
          positionX: clone.positionX,
          positionY: clone.positionY,
        });
        created++;
      } catch {
        break;
      }
    }

    qc.invalidateQueries(['ships', battleId, fractionId]);
    qc.invalidateQueries(['battle', battleId]);
    showSuccess(`Utworzono ${created} z ${data.clones.length} klonów statku.`);
    navigate(`/pustka-admin-panel/${battleId}/fractions/${fractionId}/ships`);
  });

  if (loadingShip || !templateShip) {
    return (
      <Box>
        <Spinner />
        {!templateShip && !loadingShip && <Text>Statek nie znaleziony.</Text>}
      </Box>
    );
  }

  const backToShips = `/pustka-admin-panel/${battleId}/fractions/${fractionId}/ships`;

  return (
    <Box>
      <Breadcrumb.Root mb="4" fontSize="sm">
        <Breadcrumb.List>
          <Breadcrumb.Item>
            <Breadcrumb.Link asChild>
              <Link to="/pustka-admin-panel">Battles</Link>
            </Breadcrumb.Link>
          </Breadcrumb.Item>
          <Breadcrumb.Separator />
          <Breadcrumb.Item>
            <Breadcrumb.Link asChild>
              <Link to={`/pustka-admin-panel/${battleId}`}>Battle</Link>
            </Breadcrumb.Link>
          </Breadcrumb.Item>
          <Breadcrumb.Separator />
          <Breadcrumb.Item>
            <Breadcrumb.Link asChild>
              <Link to={backToShips}>Statki</Link>
            </Breadcrumb.Link>
          </Breadcrumb.Item>
          <Breadcrumb.Separator />
          <Breadcrumb.Item>
            <Breadcrumb.CurrentLink>Klonuj: {templateShip.name}</Breadcrumb.CurrentLink>
          </Breadcrumb.Item>
        </Breadcrumb.List>
      </Breadcrumb.Root>

      <Heading size="md" mb="4">
        Klonowanie statku: {templateShip.name} ({getShipTypeNamePl(templateShip.type)})
      </Heading>

      {step === 1 && (
        <VStack as="form" align="stretch" spacing="4" onSubmit={onCountSubmit}>
          <Field.Root>
            <Field.Label>Ile razy sklonować statek?</Field.Label>
            <Input
              type="number"
              min={1}
              max={50}
              {...countForm.register('cloneCount')}
            />
            {countForm.formState.errors.cloneCount && (
              <Text color="red.500" fontSize="sm">
                {countForm.formState.errors.cloneCount.message}
              </Text>
            )}
          </Field.Root>
          <HStack>
            <Button type="submit" colorScheme="blue">
              Dalej — podaj nazwy i pozycje
            </Button>
            <Button as={Link} to={backToShips} variant="outline">
              Anuluj
            </Button>
          </HStack>
        </VStack>
      )}

      {step === 2 && (
        <VStack as="form" align="stretch" spacing="4" onSubmit={onClonesSubmit}>
          <Text color="gray.600">
            Dla każdej kopii podaj nazwę i pozycję startową (X, Y). Pozostałe parametry będą takie jak w statku wzorcowym.
          </Text>
          <VStack align="stretch" spacing="4">
            {Array.from({ length: cloneCount }, (_, index) => (
              <Box
                key={index}
                p="4"
                borderWidth="1px"
                borderRadius="md"
                bg="gray.50"
              >
                <Text fontWeight="bold" mb="2">
                  Kopia {index + 1}
                </Text>
                <SimpleGrid columns={{ base: 1, md: 3 }} spacing="3">
                  <Field.Root>
                    <Field.Label>Nazwa statku</Field.Label>
                    <Input {...clonesForm.register(`clones.${index}.name`)} />
                    {clonesForm.formState.errors.clones?.[index]?.name && (
                      <Text color="red.500" fontSize="sm">
                        {clonesForm.formState.errors.clones[index].name.message}
                      </Text>
                    )}
                  </Field.Root>
                  <Field.Root>
                    <Field.Label>Pozycja X</Field.Label>
                    <Input
                      type="number"
                      {...clonesForm.register(`clones.${index}.positionX`)}
                    />
                  </Field.Root>
                  <Field.Root>
                    <Field.Label>Pozycja Y</Field.Label>
                    <Input
                      type="number"
                      {...clonesForm.register(`clones.${index}.positionY`)}
                    />
                  </Field.Root>
                </SimpleGrid>
              </Box>
            ))}
          </VStack>
          <HStack>
            <Button
              type="button"
              variant="outline"
              onClick={() => {
                setStep(1);
                setCloneCount(0);
              }}
            >
              Wstecz
            </Button>
            <Button
              type="submit"
              colorScheme="green"
              isLoading={createMutation.isPending}
            >
              Utwórz {cloneCount} klonów
            </Button>
            <Button as={Link} to={backToShips} variant="outline">
              Anuluj
            </Button>
          </HStack>
        </VStack>
      )}
    </Box>
  );
}
